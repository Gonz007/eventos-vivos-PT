using System.Net;
using System.Net.Http.Json;
using EventBooking.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace EventBooking.IntegrationTests;

public class EventsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEvents_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/events");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateEvent_ValidPayload_Returns201()
    {
        var payload = new
        {
            title       = "Integration Test Event",
            description = "Testing the full pipeline with SQLite",
            venueId     = 1,
            maxCapacity = 50,
            startDate   = DateTime.UtcNow.AddDays(10).ToString("o"),
            endDate     = DateTime.UtcNow.AddDays(10).AddHours(3).ToString("o"),
            ticketPrice = 30.0,
            eventType   = 1
        };

        var response = await _client.PostAsJsonAsync("/api/events", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<EventDto>();
        body.Should().NotBeNull();
        body!.Title.Should().Be("Integration Test Event");
    }

    [Fact]
    public async Task CreateEvent_CapacityExceedsVenue_Returns422()
    {
        var payload = new
        {
            title       = "Over Capacity Event",
            description = "This should fail because capacity > venue",
            venueId     = 2,            // Sala Norte cap = 50
            maxCapacity = 200,          // exceeds 50
            startDate   = DateTime.UtcNow.AddDays(15).ToString("o"),
            endDate     = DateTime.UtcNow.AddDays(15).AddHours(2).ToString("o"),
            ticketPrice = 20.0,
            eventType   = 2
        };

        var response = await _client.PostAsJsonAsync("/api/events", payload);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetEventById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/events/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
