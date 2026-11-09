using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ParticipantController : Controller
{
    private readonly ApiClient _api;

    public ParticipantController(ApiClient api) => _api = api;

    private bool IsParticipant => HttpContext.Session.GetString("Role") == "Participant";

    private IActionResult RedirectToLogin(string message)
    {
        _api.ClearApiSession();
        HttpContext.Session.Clear();
        TempData["Success"] = message;
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> MyEvents()
    {
        if (!IsParticipant)
            return RedirectToLogin("Please log in as a Participant to see your events.");

        var result = await _api.GetAsync<List<EnrolmentInfo>>("api/users/me/enrolments");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ViewBag.Error = result.Error;
            return View(new List<EnrolmentInfo>());
        }

        var enrolments = (result.Data ?? new List<EnrolmentInfo>())
            .OrderByDescending(e => e.EventDate)
            .ToList();
        return View(enrolments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        if (!IsParticipant)
            return RedirectToLogin("Please log in as a Participant to manage your events.");

        var result = await _api.DeleteAsync($"api/enrolments/{id}");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (result.Success)
            TempData["Success"] = "Your enrolment has been cancelled.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(MyEvents));
    }

    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipant)
            return RedirectToLogin("Please log in as a Participant to see your results.");

        var result = await _api.GetAsync<List<MyResultInfo>>("api/users/me/results");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ViewBag.Error = result.Error;
            return View(new List<MyResultInfo>());
        }

        var results = (result.Data ?? new List<MyResultInfo>())
            .OrderByDescending(r => r.EventDate)
            .ToList();
        return View(results);
    }
}