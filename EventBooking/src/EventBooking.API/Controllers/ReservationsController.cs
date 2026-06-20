using EventBooking.Application.Reservations.Commands.CancelReservation;
using EventBooking.Application.Reservations.Commands.ConfirmPayment;
using EventBooking.Application.Reservations.Commands.CreateReservation;
using EventBooking.Application.Reservations.Queries.GetReservationById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new reservation for an event.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetReservationById), new { id = result.Id }, result);
    }

    /// <summary>Confirms payment for a reservation.</summary>
    [HttpPost("{id:int}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmPayment(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmPaymentCommand(id), ct);
        return Ok(result);
    }

    /// <summary>Cancels a reservation.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelReservation(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelReservationCommand(id), ct);
        return Ok(result);
    }

    /// <summary>Gets a reservation by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetReservationByIdQuery(id), ct);
        return Ok(result);
    }
}
