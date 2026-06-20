using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;

namespace EventBooking.UnitTests.Helpers;

/// <summary>Fluent builder for creating test Event entities.</summary>
public class EventBuilder
{
    private int _id = 1;
    private string _title = "Test Event Title";
    private string _description = "Test Event Description for testing purposes";
    private int _venueId = 1;
    private int _maxCapacity = 100;
    private DateTime _startDate = DateTime.UtcNow.AddDays(7);
    private DateTime _endDate = DateTime.UtcNow.AddDays(7).AddHours(3);
    private decimal _ticketPrice = 50m;
    private EventType _eventType = EventType.Conference;
    private EventStatus _status = EventStatus.Active;
    private Venue? _venue;

    public EventBuilder WithId(int id) { _id = id; return this; }
    public EventBuilder WithTitle(string title) { _title = title; return this; }
    public EventBuilder WithVenueId(int venueId) { _venueId = venueId; return this; }
    public EventBuilder WithMaxCapacity(int capacity) { _maxCapacity = capacity; return this; }
    public EventBuilder WithStartDate(DateTime date) { _startDate = date; return this; }
    public EventBuilder WithEndDate(DateTime date) { _endDate = date; return this; }
    public EventBuilder WithTicketPrice(decimal price) { _ticketPrice = price; return this; }
    public EventBuilder WithEventType(EventType type) { _eventType = type; return this; }
    public EventBuilder WithStatus(EventStatus status) { _status = status; return this; }
    public EventBuilder WithVenue(Venue venue) { _venue = venue; return this; }

    public Event Build() => new()
    {
        Id = _id,
        Title = _title,
        Description = _description,
        VenueId = _venueId,
        MaxCapacity = _maxCapacity,
        StartDate = _startDate,
        EndDate = _endDate,
        TicketPrice = _ticketPrice,
        EventType = _eventType,
        Status = _status,
        Venue = _venue ?? new Venue { Id = _venueId, Name = "Test Venue", Capacity = 200, City = "Bogotá" }
    };
}
