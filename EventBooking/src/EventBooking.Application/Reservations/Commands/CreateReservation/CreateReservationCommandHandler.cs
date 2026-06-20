using EventBooking.Application.DTOs;
using EventBooking.Application.Interfaces;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly IApplicationDbContext _context;

    public CreateReservationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);

        if (@event.Status != EventStatus.Active)
            throw new DomainException($"Cannot reserve tickets for an event with status '{@event.Status}'.");

        var now = DateTime.UtcNow;

        // RN-04: No reservations within 1 hour of start
        if (@event.StartDate - now < TimeSpan.FromHours(1))
            throw new DomainException("Reservations are not allowed for events starting in less than 1 hour.");

        // RF-03: Limit to max 5 tickets if less than 24 hours to start
        if (@event.StartDate - now < TimeSpan.FromHours(24) && request.Quantity > 5)
            throw new DomainException("Only a maximum of 5 tickets can be reserved when the event starts in less than 24 hours.");

        // RN-05: Premium events (price > 100) limit to max 10 tickets
        if (@event.TicketPrice > 100 && request.Quantity > 10)
            throw new DomainException("Events with a ticket price above $100 allow a maximum of 10 tickets per reservation.");

        // Calculate available tickets (confirmed reservations only consume capacity, but pending reservations also hold capacity)
        var reservedTickets = await _context.Reservations
            .Where(r => r.EventId == request.EventId &&
                        r.Status != ReservationStatus.Cancelled &&
                        !r.IsLostSale)
            .SumAsync(r => r.Quantity, cancellationToken);

        var available = @event.MaxCapacity - reservedTickets;

        if (request.Quantity > available)
            throw new DomainException($"Not enough available tickets. Available: {available}, Requested: {request.Quantity}.");

        var reservation = new Reservation
        {
            EventId = request.EventId,
            Quantity = request.Quantity,
            BuyerName = request.BuyerName,
            BuyerEmail = request.BuyerEmail,
            Status = ReservationStatus.PendingPayment,
            CreatedAt = now
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        return new ReservationDto(
            reservation.Id,
            reservation.EventId,
            @event.Title,
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
