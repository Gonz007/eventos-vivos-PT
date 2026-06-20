using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Events.Queries.GetEvents;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        // RN-06: Auto-complete events whose EndDate has passed
        var now = DateTime.UtcNow;
        var expiredEvents = await _context.Events
            .Where(e => e.Status == EventStatus.Active && e.EndDate < now)
            .ToListAsync(cancellationToken);

        foreach (var expired in expiredEvents)
            expired.Status = EventStatus.Completed;

        if (expiredEvents.Any())
            await _context.SaveChangesAsync(cancellationToken);

        var query = _context.Events
            .Include(e => e.Venue)
            .AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(e => e.EventType == request.Type.Value);

        if (request.VenueId.HasValue)
            query = query.Where(e => e.VenueId == request.VenueId.Value);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(e => e.Title.ToLower().Contains(request.Search.ToLower()));

        if (request.StartDateFrom.HasValue)
            query = query.Where(e => e.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(e => e.StartDate <= request.StartDateTo.Value);

        var events = await query.ToListAsync(cancellationToken);

        return events.Select(e => new EventDto(
            e.Id,
            e.Title,
            e.Description,
            e.VenueId,
            e.Venue.Name,
            e.MaxCapacity,
            e.StartDate,
            e.EndDate,
            e.TicketPrice,
            e.EventType.ToString(),
            e.Status.ToString()
        )).ToList();
    }
}
