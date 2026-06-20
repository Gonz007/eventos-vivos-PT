using EventBooking.Application.Events.Commands.CreateEvent;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using EventBooking.UnitTests.Helpers;
using FluentAssertions;

namespace EventBooking.UnitTests.Events;

public class CreateEventHandlerTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>Returns a command with valid defaults, overridable per test.</summary>
    private static CreateEventCommand ValidCommand(
        int venueId = 0,          // caller must override with the real ID
        int maxCapacity = 100,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal ticketPrice = 50m,
        EventType type = EventType.Conference) =>
        new(
            Title:       "Tech Conference 2027",
            Description: "A great tech conference about modern software",
            VenueId:     venueId,
            MaxCapacity: maxCapacity,
            StartDate:   startDate ?? DateTime.UtcNow.AddDays(7),
            EndDate:     endDate   ?? DateTime.UtcNow.AddDays(7).AddHours(3),
            TicketPrice: ticketPrice,
            EventType:   type
        );

    /// <summary>Inserts a Venue into the context and returns the generated entity (with DB Id).</summary>
    private static async Task<Venue> SeedVenueAsync(
        EventBooking.Infrastructure.Persistence.ApplicationDbContext ctx,
        int capacity = 200)
    {
        var venue = new Venue { Name = "Auditorio Central", Capacity = capacity, City = "Bogotá" };
        ctx.Venues.Add(venue);
        await ctx.SaveChangesAsync();
        return venue;
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEvent_Success_ReturnsEventDto()
    {
        using var ctx = DbContextFactory.Create();
        var venue   = await SeedVenueAsync(ctx);
        var handler = new CreateEventCommandHandler(ctx);

        var result = await handler.Handle(ValidCommand(venueId: venue.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Tech Conference 2027");
        result.Status.Should().Be("Active");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateEvent_VenueNotFound_ThrowsNotFoundException()
    {
        using var ctx = DbContextFactory.Create();
        var handler = new CreateEventCommandHandler(ctx);

        var act = async () => await handler.Handle(ValidCommand(venueId: 9999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*9999*");
    }

    [Fact]
    public async Task CreateEvent_CapacityExceedsVenue_ThrowsDomainException()
    {
        using var ctx = DbContextFactory.Create();
        var venue   = await SeedVenueAsync(ctx, capacity: 50);
        var handler = new CreateEventCommandHandler(ctx);

        // maxCapacity 100 > venue capacity 50
        var act = async () => await handler.Handle(ValidCommand(venueId: venue.Id, maxCapacity: 100), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cannot exceed venue capacity*");
    }

    [Fact]
    public async Task CreateEvent_VenueOverlap_ThrowsDomainException()
    {
        using var ctx = DbContextFactory.Create();
        var venue = await SeedVenueAsync(ctx);

        // Seed an existing active event (no navigation property to avoid tracking conflicts)
        ctx.Events.Add(new Event
        {
            Title       = "Existing Event",
            Description = "Already occupies this slot",
            VenueId     = venue.Id,
            MaxCapacity = 100,
            StartDate   = DateTime.UtcNow.AddDays(5),
            EndDate     = DateTime.UtcNow.AddDays(5).AddHours(4),
            TicketPrice = 50m,
            EventType   = EventType.Conference,
            Status      = EventStatus.Active
        });
        await ctx.SaveChangesAsync();

        var handler = new CreateEventCommandHandler(ctx);
        var command = ValidCommand(
            venueId:   venue.Id,
            startDate: DateTime.UtcNow.AddDays(5).AddHours(1),
            endDate:   DateTime.UtcNow.AddDays(5).AddHours(5));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*active event at this venue*");
    }

    [Fact]
    public async Task CreateEvent_WeekendAfter22_ThrowsDomainException()
    {
        using var ctx = DbContextFactory.Create();
        var venue   = await SeedVenueAsync(ctx);
        var handler = new CreateEventCommandHandler(ctx);

        // Find next Saturday at 22:00 UTC
        var today            = DateTime.UtcNow.Date;
        var daysToSaturday   = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (daysToSaturday == 0) daysToSaturday = 7;
        var nextSaturday     = today.AddDays(daysToSaturday);
        var startAt22        = new DateTime(nextSaturday.Year, nextSaturday.Month, nextSaturday.Day, 22, 0, 0, DateTimeKind.Utc);

        var act = async () => await handler.Handle(
            ValidCommand(venueId: venue.Id, startDate: startAt22, endDate: startAt22.AddHours(2)),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*weekends cannot start at or after 22:00*");
    }

    [Fact]
    public async Task CreateEvent_WeekendBefore22_Succeeds()
    {
        using var ctx = DbContextFactory.Create();
        var venue   = await SeedVenueAsync(ctx);
        var handler = new CreateEventCommandHandler(ctx);

        // Find next Saturday at 20:00 — allowed
        var today          = DateTime.UtcNow.Date;
        var daysToSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (daysToSaturday == 0) daysToSaturday = 7;
        var nextSaturday   = today.AddDays(daysToSaturday);
        var startAt20      = new DateTime(nextSaturday.Year, nextSaturday.Month, nextSaturday.Day, 20, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            ValidCommand(venueId: venue.Id, startDate: startAt20, endDate: startAt20.AddHours(1)),
            CancellationToken.None);

        result.Should().NotBeNull();
    }
}
