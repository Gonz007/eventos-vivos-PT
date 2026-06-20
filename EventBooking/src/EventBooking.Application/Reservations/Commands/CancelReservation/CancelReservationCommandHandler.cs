using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, ReservationDto>
{
    private readonly IApplicationDbContext _context;

    public CancelReservationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new DomainException("Reservation is already cancelled.");

        var now = DateTime.UtcNow;

        // RN-07: If event starts in less than 48 hours, mark as lost sale (capacity NOT released)
        var isLostSale = reservation.Event.StartDate - now < TimeSpan.FromHours(48);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = now;
        reservation.IsLostSale = isLostSale;

        await _context.SaveChangesAsync(cancellationToken);

        return new ReservationDto(
            reservation.Id,
            reservation.EventId,
            reservation.Event.Title,
            reservation.Quantity,
            reservation.BuyerName,
            reservation.BuyerEmail,
            reservation.Status.ToString(),
            reservation.ReservationCode,
            reservation.CreatedAt,
            reservation.CancelledAt,
            reservation.IsLostSale
        );
    }
}
