using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDto>
{
    private readonly IApplicationDbContext _context;

    public GetReservationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

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
