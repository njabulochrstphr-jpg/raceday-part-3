using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrolmentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EnrolmentsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Enrol_AsParticipant_IsCorrectlyRecorded()
    {
        // Set up: an Organiser creates an event and a category, and
        // attaches the category to the event.
        var organiserClient = _factory.CreateClient();
        var organiserEmail = $"enroltest-organiser{Guid.NewGuid()}@raceday.com";
        await organiserClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Organiser", organiserEmail, "Password123!", "Organiser"));
        await organiserClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(organiserEmail, "Password123!"));

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events",
            new CreateEventRequest("Enrol Test Event", "desc", DateTime.UtcNow.AddMonths(1), "Test Location", 10, "run"));
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();

        var categoryResponse = await organiserClient.PostAsJsonAsync("/api/categories", new CreateCategoryRequest($"10km-{Guid.NewGuid()}"));
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var attachResponse = await organiserClient.PostAsJsonAsync($"/api/events/{createdEvent!.Id}/categories",
            new AttachCategoryRequest(createdCategory!.Id));
        var eventCategory = await attachResponse.Content.ReadFromJsonAsync<EventCategoryResponse>();

        // Act: a Participant enrols in that event category.
        var participantClient = _factory.CreateClient();
        var participantEmail = $"enroltest-participant{Guid.NewGuid()}@raceday.com";
        await participantClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Participant", participantEmail, "Password123!", "Participant"));
        await participantClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(participantEmail, "Password123!"));

        var enrolResponse = await participantClient.PostAsync($"/api/event-categories/{eventCategory!.Id}/enrol", null);
        Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);

        // Assert: the enrolment is correctly recorded and retrievable.
        var myEnrolments = await participantClient.GetFromJsonAsync<List<EnrolmentResponse>>("/api/users/me/enrolments");
        Assert.Single(myEnrolments!);
        Assert.Equal("Enrol Test Event", myEnrolments![0].EventTitle);
    }

    [Fact]
    public async Task Enrol_SameEventCategoryTwice_ReturnsConflict()
    {
        var organiserClient = _factory.CreateClient();
        var organiserEmail = $"dupeenrol-organiser{Guid.NewGuid()}@raceday.com";
        await organiserClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Organiser", organiserEmail, "Password123!", "Organiser"));
        await organiserClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(organiserEmail, "Password123!"));

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events",
            new CreateEventRequest("Dupe Test Event", "desc", DateTime.UtcNow.AddMonths(1), "Test Location", 10, "run"));
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();

        var categoryResponse = await organiserClient.PostAsJsonAsync("/api/categories", new CreateCategoryRequest($"5km-{Guid.NewGuid()}"));
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var attachResponse = await organiserClient.PostAsJsonAsync($"/api/events/{createdEvent!.Id}/categories",
            new AttachCategoryRequest(createdCategory!.Id));
        var eventCategory = await attachResponse.Content.ReadFromJsonAsync<EventCategoryResponse>();

        var participantClient = _factory.CreateClient();
        var participantEmail = $"dupeenrol-participant{Guid.NewGuid()}@raceday.com";
        await participantClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Participant", participantEmail, "Password123!", "Participant"));
        await participantClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(participantEmail, "Password123!"));

        await participantClient.PostAsync($"/api/event-categories/{eventCategory!.Id}/enrol", null);
        var secondAttempt = await participantClient.PostAsync($"/api/event-categories/{eventCategory!.Id}/enrol", null);

        Assert.Equal(HttpStatusCode.Conflict, secondAttempt.StatusCode);
    }
}
