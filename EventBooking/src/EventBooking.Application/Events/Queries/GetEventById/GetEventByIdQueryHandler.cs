using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.Id);

        // RN-06: Auto-complete if past end date
        if (@event.Status == EventStatus.Active && @event.EndDate < DateTime.UtcNow)
        {
            @event.Status = EventStatus.Completed;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new EventDto(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.VenueId,
            @event.Venue.Name,
            @event.MaxCapacity,
            @event.StartDate,
            @event.EndDate,
            @event.TicketPrice,
            @event.EventType.ToString(),
            @event.Status.ToString()
        );
    }
}
