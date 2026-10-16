using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Tests;

public class AuthenticationIntegrationTests
: IClassFixture<RaceDayWebApplicationFactory>
{
    private readonly RaceDayWebApplicationFactory _factory;
    private readonly HttpClient _client;

public AuthenticationIntegrationTests(
    RaceDayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.InitializeDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Events_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Events");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Results_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Results");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_ReturnsForbidden()
    {
        await LoginAsAsync(
            "participant.integration@example.com",
            "ParticipantTest2026!");

        var response = await _client.PostAsJsonAsync("/api/Events", new
        {
            EventName = "Integration Test Event",
            Event_Discrip = "Created during an integration test",
            EventDate = DateTime.UtcNow.AddDays(30),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateResult_ReturnsForbidden()
    {
        await LoginAsAsync(
            "participant.integration@example.com",
            "ParticipantTest2026!");

        var response = await _client.PostAsJsonAsync("/api/Results", new
        {
            EnrolmentID = 1,
            FinishTime = "00:45:32",
            FinishPosition = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanAccessEvents_ReturnsSuccess()
    {
        await LoginAsAsync(
            "organiser.integration@example.com",
            "OrganiserTest2026!");

        var response = await _client.GetAsync("/api/Events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task LoginAsAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    private sealed class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
    }

}
