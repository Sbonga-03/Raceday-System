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

    public async Task<IActionResult> Events()
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage events.");

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

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to create events.");

        return View(new EventFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to create events.");

        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<EventSummary>("api/events", new
        {
            name = model.Name,
            eventDate = model.EventDate,
            location = model.Location,
            description = model.Description,
            eventType = model.EventType,
            distance = model.Distance
        });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be created.");
            return View(model);
        }

        TempData["Success"] = "Your event has been created.";

        if (model.Banner != null && model.Banner.Length > 0)
        {
            var upload = await _api.UploadAsync<EventSummary>($"api/events/{result.Data.EventId}/banner", model.Banner);
            if (!upload.Success)
                TempData["Error"] = "The event was created, but the banner could not be uploaded: "
                    + upload.Error + " You can add it by editing the event.";
        }

        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to edit events.");

        var result = await _api.GetAsync<EventDetail>($"api/events/{id}");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.StatusCode == HttpStatusCode.NotFound
                ? "That event could not be found."
                : result.Error;
            return RedirectToAction(nameof(Events));
        }

        var ev = result.Data;
        if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
        {
            TempData["Error"] = "You can only edit events that you created.";
            return RedirectToAction(nameof(Events));
        }

        return View(new EventFormViewModel
        {
            EventId = ev.EventId,
            Name = ev.Name,
            EventDate = ev.EventDate,
            Location = ev.Location,
            Description = ev.Description,
            EventType = ev.EventType,
            Distance = ev.Distance,
            BannerUrl = ev.BannerUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventFormViewModel model)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to edit events.");

        if (!ModelState.IsValid) return View(model);

        var result = await _api.PutAsync<EventSummary>($"api/events/{model.EventId}", new
        {
            name = model.Name,
            eventDate = model.EventDate,
            location = model.Location,
            description = model.Description,
            eventType = model.EventType,
            distance = model.Distance
        });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be updated.");
            return View(model);
        }

        TempData["Success"] = "Your event has been updated.";

        if (model.Banner != null && model.Banner.Length > 0)
        {
            var upload = await _api.UploadAsync<EventSummary>($"api/events/{model.EventId}/banner", model.Banner);
            if (!upload.Success)
                TempData["Error"] = "The event was updated, but the banner could not be uploaded: " + upload.Error;
        }

        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to delete events.");

        var result = await _api.DeleteAsync($"api/events/{id}");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (result.Success)
            TempData["Success"] = "The event has been deleted.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Events));
    }
}