using EventBooking.Application.DTOs;
using MediatR;

namespace EventBooking.Application.Reservations.Queries.GetReservationById;

public record GetReservationByIdQuery(int Id) : IRequest<ReservationDto>;
