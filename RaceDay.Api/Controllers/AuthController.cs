using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using AppUser = RaceDay.Api.Models.User;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public AuthController(RaceDayDbContext db) => _db = db;

    /// <summary>Registers a new Organiser or Participant.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleID == request.RoleId);
        if (role == null || (role.RoleName != "Organiser" && role.RoleName != "Participant"))
            return BadRequest("RoleId must be an Organiser or Participant role.");

        var email = request.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict("Email already registered.");

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleID = role.RoleID
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new UserResponse
        {
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = role.RoleName
        };
        return Created($"/api/users/me", response);
    }

    /// <summary>Logs in and stores the user ID and role in the session.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        HttpContext.Session.SetInt32("UserId", user.UserID);
        HttpContext.Session.SetString("Role", user.Role!.RoleName);

        return Ok(new UserResponse
        {
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.RoleName
        });
    }

    /// <summary>Logs out by clearing the session.</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok("Logged out.");
    }
}