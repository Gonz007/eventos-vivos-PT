namespace EventBooking.Application.DTOs;

public record EventDto(
    int Id,
    string Title,
    string Description,
    int VenueId,
    string VenueName,
    int MaxCapacity,
    DateTime StartDate,
    DateTime EndDate,
    decimal TicketPrice,
    string EventType,
    string Status
);
