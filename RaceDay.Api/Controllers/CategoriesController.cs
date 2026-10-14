using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    /// <summary>Updates a category belonging to an event the Organiser owns.</summary>
    [HttpPut("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var category = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound("Category not found.");
        if (category.Event!.OrganiserID != CurrentUserId)
            return StatusCode(403, "You do not own this event.");

        category.Name = request.Name.Trim();
        category.Distance = request.Distance;
        category.Fee = request.Fee;
        await _db.SaveChangesAsync();

        return Ok(CategoryResponse.From(category));
    }

    /// <summary>Removes a category (only if nobody has enrolled in it).</summary>
    [HttpDelete("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound("Category not found.");
        if (category.Event!.OrganiserID != CurrentUserId)
            return StatusCode(403, "You do not own this event.");

        if (await _db.EventEnrolments.AnyAsync(e => e.CategoryID == id))
            return Conflict("This category has enrolments and cannot be deleted.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}