using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.DTOs;

namespace RaceDay.Tests;

public class EnrolmentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EnrolmentTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient organiser, EventResponse ev, CategoryResponse category)> SetUpEventAsync()
    {
        var organiser = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);
        var ev = await TestHelpers.CreateEventAsync(organiser);
        var category = await TestHelpers.CreateCategoryAsync(organiser, ev.EventId);
        return (organiser, ev, category);
    }

    [Fact]
    public async Task Enrol_AsParticipant_Returns201()
    {
        var (_, _, category) = await SetUpEventAsync();
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);

        var response = await participant.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_Twice_Returns409()
    {
        var (_, _, category) = await SetUpEventAsync();
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);

        await participant.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);
        var second = await participant.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Enrol_AsOrganiser_Returns403()
    {
        var (organiser, _, category) = await SetUpEventAsync();

        var response = await organiser.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_CategoryDoesNotExist_Returns404()
    {
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);

        var response = await participant.PostAsync("/api/categories/99999/enrol", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_AsOwner_Returns201()
    {
        var (organiser, _, category) = await SetUpEventAsync();
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);
        var enrolResponse = await participant.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);
        var enrolment = await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponse>();

        var response = await organiser.PostAsJsonAsync($"/api/enrolments/{enrolment!.EnrolmentId}/result",
            new { finishTime = "00:48:32", position = 1, status = "Finished" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_Twice_Returns409()
    {
        var (organiser, _, category) = await SetUpEventAsync();
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);
        var enrolResponse = await participant.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);
        var enrolment = await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponse>();
        var body = new { finishTime = "00:48:32", position = 1, status = "Finished" };

        await organiser.PostAsJsonAsync($"/api/enrolments/{enrolment!.EnrolmentId}/result", body);
        var second = await organiser.PostAsJsonAsync($"/api/enrolments/{enrolment.EnrolmentId}/result", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task GetLeaderboard_AfterResult_ReturnsResult()
    {
        var (organiser, ev, category) = await SetUpEventAsync();
        var participant = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.ParticipantRole);
        var enrolResponse = await participant.PostAsync($"/api/categories/{category.CategoryId}/enrol", null);
        var enrolment = await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponse>();
        await organiser.PostAsJsonAsync($"/api/enrolments/{enrolment!.EnrolmentId}/result",
            new { finishTime = "00:48:32", position = 1, status = "Finished" });

        var response = await _factory.CreateClient().GetAsync($"/api/events/{ev.EventId}/results");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<ResultResponse>>();
        Assert.Single(results!);
    }
}