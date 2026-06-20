using EventBooking.Application.Events.Commands.CreateEvent;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using EventBooking.UnitTests.Helpers;
using FluentAssertions;

namespace EventBooking.UnitTests.Events;

public class CreateEventHandlerTests
{
    private static CreateEventCommand ValidCommand(
        int venueId = 1,
        int maxCapacity = 100,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal ticketPrice = 50m,
        EventType type = EventType.Conference) =>
        new(
            Title: "Tech Conference 2027",
            Description: "A great tech conference about modern software",
            VenueId: venueId,
            MaxCapacity: maxCapacity,
            StartDate: startDate ?? DateTime.UtcNow.AddDays(7),
            EndDate: endDate ?? DateTime.UtcNow.AddDays(7).AddHours(3),
            TicketPrice: ticketPrice,
            EventType: type
        );

    private static Venue DefaultVenue(int id = 1, int capacity = 200) =>
        new() { Id = id, Name = "Auditorio Central", Capacity = capacity, City = "Bogotá" };

    [Fact]
    public async Task CreateEvent_Success_ReturnsEventDto()
    {
        // Arrange
        using var ctx = DbContextFactory.Create();
        ctx.Venues.Add(DefaultVenue());
        await ctx.SaveChangesAsync();

        var handler = new CreateEventCommandHandler(ctx);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Tech Conference 2027");
        result.Status.Should().Be("Active");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateEvent_VenueNotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var ctx = DbContextFactory.Create();
        var handler = new CreateEventCommandHandler(ctx);

        // Act
        var act = async () => await handler.Handle(ValidCommand(venueId: 999), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task CreateEvent_CapacityExceedsVenue_ThrowsDomainException()
    {
        // Arrange
        using var ctx = DbContextFactory.Create();
        ctx.Venues.Add(DefaultVenue(capacity: 50));
        await ctx.SaveChangesAsync();

        var handler = new CreateEventCommandHandler(ctx);
        var command = ValidCommand(maxCapacity: 100); // venue only fits 50

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cannot exceed venue capacity*");
    }

    [Fact]
    public async Task CreateEvent_VenueOverlap_ThrowsDomainException()
    {
        // Arrange
        using var ctx = DbContextFactory.Create();
        ctx.Venues.Add(DefaultVenue());
        var existingEvent = new EventBuilder()
            .WithVenueId(1)
            .WithStartDate(DateTime.UtcNow.AddDays(5))
            .WithEndDate(DateTime.UtcNow.AddDays(5).AddHours(4))
            .Build();
        ctx.Events.Add(existingEvent);
        await ctx.SaveChangesAsync();

        var handler = new CreateEventCommandHandler(ctx);

        // New event overlaps with existing (same venue, overlapping times)
        var command = ValidCommand(
            venueId: 1,
            startDate: DateTime.UtcNow.AddDays(5).AddHours(1),
            endDate: DateTime.UtcNow.AddDays(5).AddHours(5));

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*overlapping*");
    }

    [Fact]
    public async Task CreateEvent_WeekendAfter22_ThrowsDomainException()
    {
        // Arrange
        using var ctx = DbContextFactory.Create();
        ctx.Venues.Add(DefaultVenue());
        await ctx.SaveChangesAsync();

        // Find next Saturday
        var today = DateTime.UtcNow.Date;
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilSaturday == 0) daysUntilSaturday = 7;
        var nextSaturday = today.AddDays(daysUntilSaturday);
        var startAt22 = new DateTime(nextSaturday.Year, nextSaturday.Month, nextSaturday.Day, 22, 0, 0, DateTimeKind.Utc);

        var handler = new CreateEventCommandHandler(ctx);
        var command = ValidCommand(startDate: startAt22, endDate: startAt22.AddHours(2));

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*weekends cannot start at or after 22:00*");
    }

    [Fact]
    public async Task CreateEvent_WeekendBefore22_Succeeds()
    {
        // Arrange
        using var ctx = DbContextFactory.Create();
        ctx.Venues.Add(DefaultVenue());
        await ctx.SaveChangesAsync();

        // Find next Saturday at 20:00 — should be allowed
        var today = DateTime.UtcNow.Date;
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilSaturday == 0) daysUntilSaturday = 7;
        var nextSaturday = today.AddDays(daysUntilSaturday);
        var startAt20 = new DateTime(nextSaturday.Year, nextSaturday.Month, nextSaturday.Day, 20, 0, 0, DateTimeKind.Utc);

        var handler = new CreateEventCommandHandler(ctx);
        var command = ValidCommand(startDate: startAt20, endDate: startAt20.AddHours(1));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
