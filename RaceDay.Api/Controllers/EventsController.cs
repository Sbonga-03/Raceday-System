using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    /// <summary>Lists all events, with optional filters.</summary>
    /// <param name="date">Optional. Only events on this date.</param>
    /// <param name="location">Optional. Only events whose location contains this text.</param>
    /// <response code="200">Array of events.</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? date, [FromQuery] string? location)
    {
        var query = _db.Events.AsQueryable();

        if (date.HasValue)
            query = query.Where(e => e.EventDate == date.Value.Date);
        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(e => e.Location.Contains(location));

        var events = await query.OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(EventResponse.From));
    }

    /// <summary>Returns one event including its categories.</summary>
    /// <param name="id">The event ID.</param>
    /// <response code="200">Event details with categories.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound("Event not found.");

        return Ok(new EventDetailResponse
        {
            EventId = ev.EventID,
            OrganiserId = ev.OrganiserID,
            Name = ev.Name,
            EventDate = ev.EventDate,
            Location = ev.Location,
            Description = ev.Description,
            Categories = ev.Categories.Select(CategoryResponse.From).ToList()
        });
    }

    /// <summary>Creates an event owned by the logged-in Organiser.</summary>
    /// <remarks>Body: name, eventDate, location, description.</remarks>
    /// <response code="201">Event created.</response>
    /// <response code="400">Invalid or missing fields.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create(EventRequest request)
    {
        var ev = new Event
        {
            OrganiserID = CurrentUserId,
            Name = request.Name.Trim(),
            EventDate = request.EventDate.Date,
            Location = request.Location.Trim(),
            Description = request.Description?.Trim()
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, EventResponse.From(ev));
    }

    /// <summary>Updates an event the Organiser owns.</summary>
    /// <param name="id">The event ID.</param>
    /// <remarks>Body: name, eventDate, location, description.</remarks>
    /// <response code="200">Updated event.</response>
    /// <response code="400">Invalid or missing fields.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpPut("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound("Event not found.");
        if (ev.OrganiserID != CurrentUserId) return StatusCode(403, "You do not own this event.");

        ev.Name = request.Name.Trim();
        ev.EventDate = request.EventDate.Date;
        ev.Location = request.Location.Trim();
        ev.Description = request.Description?.Trim();
        await _db.SaveChangesAsync();

        return Ok(EventResponse.From(ev));
    }

    /// <summary>Deletes an event the Organiser owns (only if it has no categories).</summary>
    /// <param name="id">The event ID.</param>
    /// <response code="204">Event deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of this event.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="409">The event still has categories.</response>
    [HttpDelete("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound("Event not found.");
        if (ev.OrganiserID != CurrentUserId) return StatusCode(403, "You do not own this event.");

        if (await _db.Categories.AnyAsync(c => c.EventID == id))
            return Conflict("Delete this event's categories first.");

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Lists all categories for an event.</summary>
    /// <param name="id">The event ID.</param>
    /// <response code="200">Array of categories.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("{id}/categories")]
    public async Task<IActionResult> GetCategories(int id)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == id))
            return NotFound("Event not found.");

        var categories = await _db.Categories.Where(c => c.EventID == id).ToListAsync();
        return Ok(categories.Select(CategoryResponse.From));
    }

    /// <summary>Adds a category to an event the Organiser owns.</summary>
    /// <param name="id">The event ID.</param>
    /// <remarks>Body: name, distance, fee.</remarks>
    /// <response code="201">Category created.</response>
    /// <response code="400">Invalid or missing fields.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpPost("{id}/categories")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> AddCategory(int id, CategoryRequest request)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound("Event not found.");
        if (ev.OrganiserID != CurrentUserId) return StatusCode(403, "You do not own this event.");

        var category = new Category
        {
            EventID = id,
            Name = request.Name.Trim(),
            Distance = request.Distance,
            Fee = request.Fee
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{id}/categories", CategoryResponse.From(category));
    }
}