using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db) => _db = db;

    /// <summary>Returns the profile of the logged-in user.</summary>
    [HttpGet("me")]
    [RequireRole]
    public async Task<IActionResult> GetMe()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return Unauthorized("User no longer exists.");

        return Ok(new UserResponse
        {
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role!.RoleName
        });
    }

    /// <summary>Updates the full name and email of the logged-in user.</summary>
    [HttpPut("me")]
    [RequireRole]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return Unauthorized("User no longer exists.");

        var email = request.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserID != user.UserID))
            return Conflict("That email is already used by another account.");

        user.FullName = request.FullName.Trim();
        user.Email = email;
        await _db.SaveChangesAsync();

        return Ok(new UserResponse
        {
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role!.RoleName
        });
    }
}