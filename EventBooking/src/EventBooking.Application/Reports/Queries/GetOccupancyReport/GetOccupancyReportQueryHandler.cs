using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reports.Queries.GetOccupancyReport;

public class GetOccupancyReportQueryHandler : IRequestHandler<GetOccupancyReportQuery, OccupancyReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetOccupancyReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OccupancyReportDto> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);

        // RN-06: Auto-complete if past end date
        if (@event.Status == EventStatus.Active && @event.EndDate < DateTime.UtcNow)
        {
            @event.Status = EventStatus.Completed;
            await _context.SaveChangesAsync(cancellationToken);
        }

        // SoldTickets = confirmed reservations only
        var soldTickets = await _context.Reservations
            .Where(r => r.EventId == request.EventId && r.Status == ReservationStatus.Confirmed)
            .SumAsync(r => (int?)r.Quantity, cancellationToken) ?? 0;

        var availableTickets = @event.MaxCapacity - soldTickets;
        var occupancyPercentage = @event.MaxCapacity > 0
            ? Math.Round((decimal)soldTickets * 100 / @event.MaxCapacity, 2)
            : 0m;
        var totalRevenue = soldTickets * @event.TicketPrice;

        return new OccupancyReportDto(
            @event.Id,
            @event.Title,
            soldTickets,
            availableTickets,
            occupancyPercentage,
            totalRevenue,
            @event.Status.ToString()
        );
    }
}
