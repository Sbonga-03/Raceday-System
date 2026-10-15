using System.Net.Http.Json;
using RaceDay.Api.DTOs;

namespace RaceDay.Tests;

public static class TestHelpers
{
    public const int OrganiserRole = 1;
    public const int ParticipantRole = 2;
    public const int AdminRole = 3;

    public static string UniqueEmail() => $"user_{Guid.NewGuid():N}@example.com";

    /// <summary>Registers a new user, logs them in, and returns a client holding their session.</summary>
    public static async Task<HttpClient> LoginNewUserAsync(CustomWebApplicationFactory factory, int roleId)
    {
        var client = factory.CreateClient();
        var email = UniqueEmail();

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { fullName = "Test User", email, password = "Password123", roleId });
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password123" });
        login.EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<EventResponse> CreateEventAsync(HttpClient organiser)
    {
        var response = await organiser.PostAsJsonAsync("/api/events", new
        {
            name = "Test Run",
            eventDate = "2026-12-20",
            location = "Pretoria",
            description = "A test event"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }

    public static async Task<CategoryResponse> CreateCategoryAsync(HttpClient organiser, int eventId)
    {
        var response = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new { name = "5km", distance = 5, fee = 100 });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }
}