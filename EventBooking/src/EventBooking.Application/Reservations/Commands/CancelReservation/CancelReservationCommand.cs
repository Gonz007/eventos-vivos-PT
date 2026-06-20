using EventBooking.Application.DTOs;
using MediatR;

namespace EventBooking.Application.Reservations.Commands.CancelReservation;

public record CancelReservationCommand(int ReservationId) : IRequest<ReservationDto>;
