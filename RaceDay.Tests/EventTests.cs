using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Tests;

public class EventTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EventTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateEvent_AsOrganiser_Returns201()
    {
        var organiser = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);

        var response = await organiser.PostAsJsonAsync("/api/events", new
        {
            name = "City Run",
            eventDate = "2026-12-20",
            location = "Johannesburg",
            description = "Annual race"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_Returns403()
    {
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);

        var response = await participant.PostAsJsonAsync("/api/events", new
        {
            name = "City Run",
            eventDate = "2026-12-20",
            location = "Johannesburg"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_NotLoggedIn_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            name = "City Run",
            eventDate = "2026-12-20",
            location = "Johannesburg"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_ByDifferentOrganiser_Returns403()
    {
        var owner = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);
        var other = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);
        var ev = await TestHelpers.CreateEventAsync(owner);

        var response = await other.PutAsJsonAsync($"/api/events/{ev.EventId}", new
        {
            name = "Hijacked",
            eventDate = "2026-12-21",
            location = "Nowhere"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_DoesNotExist_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_AsOwner_Returns201()
    {
        var organiser = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);
        var ev = await TestHelpers.CreateEventAsync(organiser);

        var response = await organiser.PostAsJsonAsync($"/api/events/{ev.EventId}/categories",
            new { name = "10km", distance = 10, fee = 150 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_WithCategories_Returns409()
    {
        var organiser = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);
        var ev = await TestHelpers.CreateEventAsync(organiser);
        await TestHelpers.CreateCategoryAsync(organiser, ev.EventId);

        var response = await organiser.DeleteAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsOwner_Returns204()
    {
        var organiser = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);
        var ev = await TestHelpers.CreateEventAsync(organiser);

        var response = await organiser.DeleteAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}