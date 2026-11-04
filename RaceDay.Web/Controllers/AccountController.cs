using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<UserInfo>("api/auth/login",
            new { email = model.Email, password = model.Password });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed. Please try again.");
            return View(model);
        }

        var user = result.Data;
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("ProfilePictureUrl", user.ProfilePictureUrl ?? string.Empty);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<UserInfo>("api/auth/register", new
        {
            fullName = model.FullName,
            email = model.Email,
            password = model.Password,
            roleId = model.RoleId
        });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed. Please try again.");
            return View(model);
        }

        TempData["Success"] = "Your account has been created. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _api.PostAsync<bool>("api/auth/logout");
        _api.ClearApiSession();
        HttpContext.Session.Clear();

        TempData["Success"] = "You have been logged out.";
        return RedirectToAction(nameof(Login));
    }
}