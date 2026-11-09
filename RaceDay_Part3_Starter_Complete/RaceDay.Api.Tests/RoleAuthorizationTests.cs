using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests;

public class RoleAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoleAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var request = new CreateEventRequest("Test Event", "desc", DateTime.UtcNow.AddMonths(1), "Test Location", 10, "run");

        var response = await client.PostAsJsonAsync("/api/events", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        // Different roles are rejected when attempting role-based functionality:
        // a Participant must not be able to create an event.
        var client = _factory.CreateClient();
        var email = $"participant{Guid.NewGuid()}@raceday.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Participant User", email, "Password123!", "Participant"));
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));

        var request = new CreateEventRequest("Test Event", "desc", DateTime.UtcNow.AddMonths(1), "Test Location", 10, "run");
        var response = await client.PostAsJsonAsync("/api/events", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        // Verifies that Organisers CAN manage events.
        var client = _factory.CreateClient();
        var email = $"organiser{Guid.NewGuid()}@raceday.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Organiser User", email, "Password123!", "Organiser"));
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));

        var request = new CreateEventRequest("Test Event", "desc", DateTime.UtcNow.AddMonths(1), "Test Location", 10, "run");
        var response = await client.PostAsJsonAsync("/api/events", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task RecordResult_AsParticipant_ReturnsForbidden()
    {
        // Organiser-only functionality (recording a result) must reject Participants.
        var client = _factory.CreateClient();
        var email = $"resultparticipant{Guid.NewGuid()}@raceday.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Participant", email, "Password123!", "Participant"));
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));

        var response = await client.PostAsJsonAsync("/api/enrolments/1/result", new CreateResultRequest("00:45:00", 1));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
