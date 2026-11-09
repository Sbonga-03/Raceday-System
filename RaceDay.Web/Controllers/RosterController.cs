using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class RosterController : Controller
{
    private static readonly string[] AllowedStatuses = { "Finished", "DNF", "DNS" };

    private readonly ApiClient _api;

    public RosterController(ApiClient api) => _api = api;

    private bool IsOrganiser => HttpContext.Session.GetString("Role") == "Organiser";

    private IActionResult RedirectToLogin(string message)
    {
        _api.ClearApiSession();
        HttpContext.Session.Clear();
        TempData["Success"] = message;
        return RedirectToAction("Login", "Account");
    }

    // Loads an event and checks that the logged-in organiser owns it
    private async Task<(EventDetail? ev, IActionResult? redirect)> LoadOwnedEvent(int eventId)
    {
        var result = await _api.GetAsync<EventDetail>($"api/events/{eventId}");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.StatusCode == HttpStatusCode.NotFound
                ? "That event could not be found."
                : result.Error;
            return (null, RedirectToAction("Events", "Organiser"));
        }

        if (result.Data.OrganiserId != HttpContext.Session.GetInt32("UserId"))
        {
            TempData["Error"] = "You can only manage events that you created.";
            return (null, RedirectToAction("Events", "Organiser"));
        }

        return (result.Data, null);
    }

    public async Task<IActionResult> Index(int id)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to see enrolments.");

        var (ev, redirect) = await LoadOwnedEvent(id);
        if (redirect != null) return redirect;

        var result = await _api.GetAsync<List<EnrolmentInfo>>($"api/events/{id}/enrolments");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
            ViewBag.Error = result.Error;

        return View(new EventRosterViewModel
        {
            EventId = id,
            EventName = ev!.Name,
            EventDate = ev.EventDate,
            Enrolments = (result.Data ?? new List<EnrolmentInfo>())
                .OrderBy(e => e.CategoryName).ThenBy(e => e.ParticipantName).ToList()
        });
    }

    public async Task<IActionResult> Results(int id)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage results.");

        var (ev, redirect) = await LoadOwnedEvent(id);
        if (redirect != null) return redirect;

        var enrolments = await _api.GetAsync<List<EnrolmentInfo>>($"api/events/{id}/enrolments");

        if (enrolments.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        var recorded = await _api.GetAsync<List<RecordedResult>>($"api/events/{id}/results");

        if (!enrolments.Success) ViewBag.Error = enrolments.Error;
        else if (!recorded.Success) ViewBag.Error = recorded.Error;

        return View(new ResultsPageViewModel
        {
            EventId = id,
            EventName = ev!.Name,
            EventDate = ev.EventDate,
            IsConcluded = ev.EventDate.Date <= DateTime.Today,
            Pending = (enrolments.Data ?? new List<EnrolmentInfo>())
                .Where(e => !e.HasResult)
                .OrderBy(e => e.CategoryName).ThenBy(e => e.ParticipantName).ToList(),
            Recorded = recorded.Data ?? new List<RecordedResult>()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(ResultEntryViewModel model)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage results.");

        var (ev, redirect) = await LoadOwnedEvent(model.EventId);
        if (redirect != null) return redirect;

        IActionResult Back(string error)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Results), new { id = model.EventId });
        }

        if (ev!.EventDate.Date > DateTime.Today)
            return Back("Results can only be entered once the event has taken place.");

        var status = model.Status ?? "Finished";
        if (!AllowedStatuses.Contains(status))
            return Back("Please choose Finished, DNF or DNS as the status.");

        TimeSpan? finishTime = null;
        if (!string.IsNullOrWhiteSpace(model.FinishTime))
        {
            if (!TimeSpan.TryParseExact(model.FinishTime.Trim(), new[] { @"h\:mm\:ss", @"hh\:mm\:ss" },
                    CultureInfo.InvariantCulture, out var parsed))
                return Back("Please enter the finish time as hh:mm:ss, for example 00:48:32.");
            finishTime = parsed;
        }

        if (model.Position.HasValue && model.Position.Value < 1)
            return Back("The finishing position must be 1 or higher.");

        if (status == "Finished" && (finishTime == null || model.Position == null))
            return Back("A finisher needs both a finish time and a finishing position.");

        if (status != "Finished")
        {
            finishTime = null;
            model.Position = null;
        }

        var result = await _api.PostAsync<RecordedResult>($"api/enrolments/{model.EnrolmentId}/result", new
        {
            finishTime,
            position = model.Position,
            status
        });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
            return Back(result.Error ?? "The result could not be saved.");

        TempData["Success"] = "The result has been saved and published.";
        return RedirectToAction(nameof(Results), new { id = model.EventId });
    }
}