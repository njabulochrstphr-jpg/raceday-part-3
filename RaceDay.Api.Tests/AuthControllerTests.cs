using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("Test User", $"test{Guid.NewGuid()}@raceday.com", "Password123!", "Participant");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var email = $"dupe{Guid.NewGuid()}@raceday.com";
        var request = new RegisterRequest("Test User", email, "Password123!", "Participant");

        await client.PostAsJsonAsync("/api/auth/register", request);
        var secondResponse = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("Test User", $"badrole{Guid.NewGuid()}@raceday.com", "Password123!", "Admin");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var email = $"login{Guid.NewGuid()}@raceday.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Login User", email, "Password123!", "Participant"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var email = $"wrongpass{Guid.NewGuid()}@raceday.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Login User", email, "Password123!", "Participant"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
