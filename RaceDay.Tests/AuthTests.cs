using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.DTOs;

namespace RaceDay.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_ValidOrganiser_Returns201()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "New Organiser",
            email = TestHelpers.UniqueEmail(),
            password = "Password123",
            roleId = TestHelpers.OrganiserRole
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Organiser", user!.Role);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            fullName = "Duplicate User",
            email = TestHelpers.UniqueEmail(),
            password = "Password123",
            roleId = TestHelpers.ParticipantRole
        };

        await client.PostAsJsonAsync("/api/auth/register", body);
        var second = await client.PostAsJsonAsync("/api/auth/register", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Bad Email",
            email = "notanemail",
            password = "Password123",
            roleId = TestHelpers.ParticipantRole
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_AdminRole_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Sneaky Admin",
            email = TestHelpers.UniqueEmail(),
            password = "Password123",
            roleId = TestHelpers.AdminRole
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Login User",
            email,
            password = "Password123",
            roleId = TestHelpers.ParticipantRole
        });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Login User",
            email,
            password = "Password123",
            roleId = TestHelpers.ParticipantRole
        });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_NotLoggedIn_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_LoggedIn_Returns200WithRole()
    {
        var client = await TestHelpers.LoginNewUserAsync(_factory, TestHelpers.OrganiserRole);

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Organiser", user!.Role);
    }
}