using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiClient _api;

    public HomeController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? location, DateTime? date)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(location))
            query.Add($"location={Uri.EscapeDataString(location.Trim())}");
        if (date.HasValue)
            query.Add($"date={date.Value:yyyy-MM-dd}");

        var url = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var result = await _api.GetAsync<List<EventSummary>>(url);

        var model = new EventsIndexViewModel { Location = location, Date = date };

        if (!result.Success)
        {
            model.Error = result.Error;
        }
        else
        {
            var events = result.Data ?? new List<EventSummary>();

            // With no date chosen, show upcoming events only
            if (!date.HasValue)
                events = events.Where(e => e.EventDate.Date >= DateTime.Today).ToList();

            model.Events = events.OrderBy(e => e.EventDate).ToList();
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}