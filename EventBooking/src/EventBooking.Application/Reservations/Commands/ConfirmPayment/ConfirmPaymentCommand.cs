using EventBooking.Application.DTOs;
using MediatR;

namespace EventBooking.Application.Reservations.Commands.ConfirmPayment;

public record ConfirmPaymentCommand(int ReservationId) : IRequest<ReservationDto>;
