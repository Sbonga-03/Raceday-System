using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ProfileController : Controller
{
    private readonly ApiClient _api;

    public ProfileController(ApiClient api) => _api = api;

    private bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Role"));

    private IActionResult RedirectToLogin(string message)
    {
        _api.ClearApiSession();
        HttpContext.Session.Clear();
        TempData["Success"] = message;
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn)
            return RedirectToLogin("Please log in to see your profile.");

        var result = await _api.GetAsync<UserInfo>("api/users/me");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Home");
        }

        var user = result.Data;
        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!IsLoggedIn)
            return RedirectToLogin("Please log in to update your profile.");

        // Role and picture are not form fields, so refill them from the session
        model.Role = HttpContext.Session.GetString("Role") ?? string.Empty;
        model.ProfilePictureUrl = HttpContext.Session.GetString("ProfilePictureUrl");

        if (!ModelState.IsValid)
            return View("Index", model);

        var result = await _api.PutAsync<UserInfo>("api/users/me",
            new { fullName = model.FullName, email = model.Email });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Your profile could not be updated.");
            return View("Index", model);
        }

        HttpContext.Session.SetString("FullName", result.Data?.FullName ?? model.FullName);
        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        if (!IsLoggedIn)
            return RedirectToLogin("Please log in to update your profile.");

        if (picture == null || picture.Length == 0)
        {
            TempData["Error"] = "Please choose an image to upload.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.UploadAsync<PictureResult>("api/users/me/picture", picture);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        HttpContext.Session.SetString("ProfilePictureUrl", result.Data?.ProfilePictureUrl ?? string.Empty);
        TempData["Success"] = "Your profile picture has been updated.";
        return RedirectToAction(nameof(Index));
    }
}