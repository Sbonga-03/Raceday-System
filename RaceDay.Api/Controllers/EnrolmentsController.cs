using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    private IQueryable<EventEnrolment> EnrolmentsWithDetails() =>
        _db.EventEnrolments
            .Include(e => e.Participant)
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Include(e => e.Result);

    /// <summary>Enrols the logged-in Participant into a category.</summary>
    /// <param name="id">The category ID.</param>
    /// <response code="201">Enrolment created.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not a Participant.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="409">Already enrolled in this category.</response>
    [HttpPost("categories/{id}/enrol")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Enrol(int id)
    {
        var category = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound("Category not found.");

        if (await _db.EventEnrolments.AnyAsync(e => e.CategoryID == id && e.ParticipantID == CurrentUserId))
            return Conflict("You are already enrolled in this category.");

        var enrolment = new EventEnrolment
        {
            ParticipantID = CurrentUserId,
            CategoryID = id
        };
        _db.EventEnrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        enrolment.Category = category;
        enrolment.Participant = await _db.Users.FindAsync(CurrentUserId);

        return Created("/api/users/me/enrolments", EnrolmentResponse.From(enrolment));
    }

    /// <summary>Lists the logged-in Participant's enrolments.</summary>
    /// <response code="200">Array of enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not a Participant.</response>
    [HttpGet("users/me/enrolments")]
    [RequireRole("Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var enrolments = await EnrolmentsWithDetails()
            .Where(e => e.ParticipantID == CurrentUserId)
            .ToListAsync();
        return Ok(enrolments.Select(EnrolmentResponse.From));
    }

    /// <summary>Cancels the logged-in Participant's own enrolment.</summary>
    /// <param name="id">The enrolment ID.</param>
    /// <response code="204">Enrolment cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not a Participant, or not your enrolment.</response>
    /// <response code="404">Enrolment not found.</response>
    /// <response code="409">A result has already been captured.</response>
    [HttpDelete("enrolments/{id}")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _db.EventEnrolments.Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment == null) return NotFound("Enrolment not found.");
        if (enrolment.ParticipantID != CurrentUserId)
            return StatusCode(403, "This is not your enrolment.");
        if (enrolment.Result != null)
            return Conflict("An enrolment with a captured result cannot be cancelled.");

        _db.EventEnrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Lists all enrolments (the roster) for an event the Organiser owns.</summary>
    /// <param name="id">The event ID.</param>
    /// <response code="200">Array of enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("events/{id}/enrolments")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> GetRoster(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound("Event not found.");
        if (ev.OrganiserID != CurrentUserId)
            return StatusCode(403, "You do not own this event.");

        var enrolments = await EnrolmentsWithDetails()
            .Where(e => e.Category!.EventID == id)
            .ToListAsync();
        return Ok(enrolments.Select(EnrolmentResponse.From));
    }

    /// <summary>Captures a result for an enrolment in an event the Organiser owns.</summary>
    /// <param name="id">The enrolment ID.</param>
    /// <remarks>Body: finishTime (hh:mm:ss), position, status.</remarks>
    /// <response code="201">Result created.</response>
    /// <response code="400">Invalid or missing fields.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of the event.</response>
    /// <response code="404">Enrolment not found.</response>
    /// <response code="409">A result has already been captured.</response>
    [HttpPost("enrolments/{id}/result")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> CaptureResult(int id, ResultRequest request)
    {
        var enrolment = await EnrolmentsWithDetails()
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment == null) return NotFound("Enrolment not found.");
        if (enrolment.Category!.Event!.OrganiserID != CurrentUserId)
            return StatusCode(403, "You do not own this event.");
        if (enrolment.Result != null)
            return Conflict("A result has already been captured for this enrolment.");

        var result = new Result
        {
            EnrolmentID = id,
            FinishTime = request.FinishTime,
            Position = request.Position,
            Status = request.Status.Trim()
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        result.Enrolment = enrolment;
        return Created($"/api/events/{enrolment.Category.EventID}/results", ResultResponse.From(result));
    }

    /// <summary>Lists all results for an event, ordered by position.</summary>
    /// <param name="id">The event ID.</param>
    /// <response code="200">Array of results.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("events/{id}/results")]
    public async Task<IActionResult> GetLeaderboard(int id)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == id))
            return NotFound("Event not found.");

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .Where(r => r.Enrolment!.Category!.EventID == id)
            .ToListAsync();

        var ordered = results
            .OrderBy(r => r.Position == null)
            .ThenBy(r => r.Position)
            .Select(ResultResponse.From);
        return Ok(ordered);
    }
}