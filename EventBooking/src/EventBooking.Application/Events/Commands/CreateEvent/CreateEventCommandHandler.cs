using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Validate venue exists
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId, cancellationToken)
            ?? throw new NotFoundException(nameof(Venue), request.VenueId);

        // RN-01: MaxCapacity must not exceed venue capacity
        if (request.MaxCapacity > venue.Capacity)
            throw new DomainException($"Event max capacity ({request.MaxCapacity}) cannot exceed venue capacity ({venue.Capacity}).");

        // RN-03: Weekend restriction — events on Sat/Sun cannot start after 22:00
        var startLocal = request.StartDate;
        if (startLocal.DayOfWeek == DayOfWeek.Saturday || startLocal.DayOfWeek == DayOfWeek.Sunday)
        {
            if (startLocal.Hour >= 22)
                throw new DomainException("Events on weekends cannot start at or after 22:00.");
        }

        // RN-02: No overlapping active events at same venue
        var hasOverlap = await _context.Events
            .AnyAsync(e =>
                e.VenueId == request.VenueId &&
                e.Status == EventStatus.Active &&
                request.StartDate < e.EndDate &&
                request.EndDate > e.StartDate,
                cancellationToken);

        if (hasOverlap)
            throw new DomainException("There is already an active event at this venue during the requested time slot.");

        var @event = new Event
        {
            Title = request.Title,
            Description = request.Description,
            VenueId = request.VenueId,
            MaxCapacity = request.MaxCapacity,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TicketPrice = request.TicketPrice,
            EventType = request.EventType,
            Status = EventStatus.Active
        };

        _context.Events.Add(@event);
        await _context.SaveChangesAsync(cancellationToken);

        return new EventDto(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.VenueId,
            venue.Name,
            @event.MaxCapacity,
            @event.StartDate,
            @event.EndDate,
            @event.TicketPrice,
            @event.EventType.ToString(),
            @event.Status.ToString()
        );
    }
}
