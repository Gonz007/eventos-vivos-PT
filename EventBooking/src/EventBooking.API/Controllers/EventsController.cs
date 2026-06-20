using EventBooking.Application.Events.Commands.CreateEvent;
using EventBooking.Application.Events.Queries.GetEventById;
using EventBooking.Application.Events.Queries.GetEvents;
using EventBooking.Application.Reports.Queries.GetOccupancyReport;
using EventBooking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new event.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetEventById), new { id = result.Id }, result);
    }

    /// <summary>Lists events with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] EventType? type,
        [FromQuery] int? venueId,
        [FromQuery] EventStatus? status,
        [FromQuery] string? search,
        [FromQuery] DateTime? startDateFrom,
        [FromQuery] DateTime? startDateTo,
        CancellationToken ct)
    {
        var query = new GetEventsQuery(type, venueId, status, search, startDateFrom, startDateTo);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Gets a single event by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Gets the occupancy report for an event.</summary>
    [HttpGet("{id:int}/report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOccupancyReport(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOccupancyReportQuery(id), ct);
        return Ok(result);
    }
}
