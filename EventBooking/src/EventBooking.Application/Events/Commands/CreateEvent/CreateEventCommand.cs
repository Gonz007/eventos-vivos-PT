using EventBooking.Application.DTOs;
using EventBooking.Domain.Enums;
using MediatR;

namespace EventBooking.Application.Events.Commands.CreateEvent;

public record CreateEventCommand(
    string Title,
    string Description,
    int VenueId,
    int MaxCapacity,
    DateTime StartDate,
    DateTime EndDate,
    decimal TicketPrice,
    EventType EventType
) : IRequest<EventDto>;
