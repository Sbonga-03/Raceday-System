using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class OrganiserController : Controller
{
    private readonly ApiClient _api;

    public OrganiserController(ApiClient api) => _api = api;

    private bool IsOrganiser => HttpContext.Session.GetString("Role") == "Organiser";

    private IActionResult RedirectToLogin(string message)
    {
        _api.ClearApiSession();
        HttpContext.Session.Clear();
        TempData["Success"] = message;
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> Dashboard()
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to see your dashboard.");

        var result = await _api.GetAsync<List<OrganiserEventInfo>>("api/events/mine");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ViewBag.Error = result.Error;
            return View(new List<OrganiserEventInfo>());
        }

        return View((result.Data ?? new List<OrganiserEventInfo>()).OrderBy(e => e.EventDate).ToList());
    }
}