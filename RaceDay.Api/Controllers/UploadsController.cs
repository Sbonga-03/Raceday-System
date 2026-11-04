using Azure;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class UploadsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IImageStorageService _storage;

    public UploadsController(RaceDayDbContext db, IImageStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    /// <summary>Uploads a banner image for an event the Organiser owns.</summary>
    /// <param name="id">The event ID.</param>
    /// <param name="file">The image file (JPG, PNG or WebP, max 5 MB).</param>
    /// <remarks>Send as multipart/form-data with the file in a field called "file".</remarks>
    /// <response code="200">Banner stored; returns the updated event with its BannerUrl.</response>
    /// <response code="400">No file, wrong file type, or file too large.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of this event.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="502">The image storage service is unavailable.</response>
    [HttpPost("events/{id}/banner")]
    [RequireRole("Organiser")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound("Event not found.");
        if (ev.OrganiserID != CurrentUserId) return StatusCode(403, "You do not own this event.");

        try
        {
            ev.BannerUrl = await _storage.UploadImageAsync(file, "banners");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (RequestFailedException)
        {
            return StatusCode(502, "The image storage service is unavailable.");
        }

        await _db.SaveChangesAsync();
        return Ok(EventResponse.From(ev));
    }

    /// <summary>Uploads a profile picture for the logged-in user.</summary>
    /// <param name="file">The image file (JPG, PNG or WebP, max 5 MB).</param>
    /// <remarks>Send as multipart/form-data with the file in a field called "file".</remarks>
    /// <response code="200">Picture stored; returns the new profilePictureUrl.</response>
    /// <response code="400">No file, wrong file type, or file too large.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="502">The image storage service is unavailable.</response>
    [HttpPost("users/me/picture")]
    [RequireRole]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return Unauthorized("User no longer exists.");

        try
        {
            user.ProfilePictureUrl = await _storage.UploadImageAsync(file, "profiles");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (RequestFailedException)
        {
            return StatusCode(502, "The image storage service is unavailable.");
        }

        await _db.SaveChangesAsync();
        return Ok(new { profilePictureUrl = user.ProfilePictureUrl });
    }
}