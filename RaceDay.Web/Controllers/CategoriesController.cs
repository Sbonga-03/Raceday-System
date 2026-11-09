using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly ApiClient _api;

    public CategoriesController(ApiClient api) => _api = api;

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
            TempData["Error"] = "You can only manage categories for events you created.";
            return (null, RedirectToAction("Events", "Organiser"));
        }

        return (result.Data, null);
    }

    private static ManageCategoriesViewModel BuildModel(EventDetail ev, CategoryFormViewModel form) => new()
    {
        EventId = ev.EventId,
        EventName = ev.Name,
        EventDate = ev.EventDate,
        Categories = ev.Categories.OrderBy(c => c.Distance).ToList(),
        NewCategory = form
    };

    public async Task<IActionResult> Manage(int id)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage categories.");

        var (ev, redirect) = await LoadOwnedEvent(id);
        if (redirect != null) return redirect;

        return View(BuildModel(ev!, new CategoryFormViewModel { EventId = id }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CategoryFormViewModel model)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage categories.");

        var (ev, redirect) = await LoadOwnedEvent(model.EventId);
        if (redirect != null) return redirect;

        if (!ModelState.IsValid)
            return View("Manage", BuildModel(ev!, model));

        var result = await _api.PostAsync<CategoryInfo>($"api/events/{model.EventId}/categories", new
        {
            name = model.Name,
            distance = model.Distance,
            fee = model.Fee
        });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be added.");
            return View("Manage", BuildModel(ev!, model));
        }

        TempData["Success"] = "The category has been added.";
        return RedirectToAction(nameof(Manage), new { id = model.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage categories.");

        var (ev, redirect) = await LoadOwnedEvent(eventId);
        if (redirect != null) return redirect;

        var category = ev!.Categories.FirstOrDefault(c => c.CategoryId == id);
        if (category == null)
        {
            TempData["Error"] = "That category could not be found.";
            return RedirectToAction(nameof(Manage), new { id = eventId });
        }

        ViewBag.EventName = ev.Name;
        return View(new CategoryFormViewModel
        {
            EventId = eventId,
            CategoryId = category.CategoryId,
            Name = category.Name,
            Distance = category.Distance,
            Fee = category.Fee
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage categories.");

        var (ev, redirect) = await LoadOwnedEvent(model.EventId);
        if (redirect != null) return redirect;

        ViewBag.EventName = ev!.Name;
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PutAsync<CategoryInfo>($"api/categories/{model.CategoryId}", new
        {
            name = model.Name,
            distance = model.Distance,
            fee = model.Fee
        });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be updated.");
            return View(model);
        }

        TempData["Success"] = "The category has been updated.";
        return RedirectToAction(nameof(Manage), new { id = model.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        if (!IsOrganiser)
            return RedirectToLogin("Please log in as an Organiser to manage categories.");

        var result = await _api.DeleteAsync($"api/categories/{id}");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return RedirectToLogin("Your session has expired. Please log in again.");

        if (result.Success)
            TempData["Success"] = "The category has been deleted.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Manage), new { id = eventId });
    }
}