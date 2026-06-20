using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, ReservationDto>
{
    private readonly IApplicationDbContext _context;

    public ConfirmPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationDto> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.Status == ReservationStatus.Confirmed)
            throw new DomainException("Reservation is already confirmed.");

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new DomainException("Cannot confirm a cancelled reservation.");

        reservation.Status = ReservationStatus.Confirmed;
        reservation.ReservationCode = GenerateReservationCode();

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

    private static string GenerateReservationCode()
    {
        var digits = Random.Shared.Next(100000, 999999);
        return $"EV-{digits}";
    }
}
