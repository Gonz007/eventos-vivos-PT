using EventBooking.Application.DTOs;
using EventBooking.Domain.Enums;
using MediatR;

namespace EventBooking.Application.Events.Queries.GetEvents;

public record GetEventsQuery(
    EventType? Type,
    int? VenueId,
    EventStatus? Status,
    string? Search,
    DateTime? StartDateFrom,
    DateTime? StartDateTo
) : IRequest<IReadOnlyList<EventDto>>;
