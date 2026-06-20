using EventBooking.Application.DTOs;
using MediatR;

namespace EventBooking.Application.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    int EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail
) : IRequest<ReservationDto>;
