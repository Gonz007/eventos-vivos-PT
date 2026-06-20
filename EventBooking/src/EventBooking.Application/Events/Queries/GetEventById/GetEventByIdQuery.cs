using EventBooking.Application.DTOs;
using MediatR;

namespace EventBooking.Application.Events.Queries.GetEventById;

public record GetEventByIdQuery(int Id) : IRequest<EventDto>;
