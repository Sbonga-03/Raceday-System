using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api) => _api = api;

    public async Task<IActionResult> Details(int id)
    {
        var result = await _api.GetAsync<EventDetail>($"api/events/{id}");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.StatusCode == HttpStatusCode.NotFound
                ? "That event could not be found."
                : result.Error;
            return RedirectToAction("Index", "Home");
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
        {
            TempData["Success"] = "Please log in as a Participant to enter an event.";
            return RedirectToAction("Login", "Account");
        }

        var result = await _api.PostAsync<object>($"api/categories/{categoryId}/enrol");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            // The API no longer recognises this login, so start again cleanly
            _api.ClearApiSession();
            HttpContext.Session.Clear();
            TempData["Success"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        if (result.Success)
            TempData["Success"] = "You are entered! Your enrolment has been recorded.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}