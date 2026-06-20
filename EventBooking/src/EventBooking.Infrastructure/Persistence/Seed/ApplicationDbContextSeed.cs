using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Persistence.Seed;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Venues.AnyAsync())
            return;

        var venues = new List<Venue>
        {
            new() { Name = "Auditorio Central", Capacity = 200, City = "Bogotá" },
            new() { Name = "Sala Norte",        Capacity = 50,  City = "Bogotá" },
            new() { Name = "Arena Sur",         Capacity = 500, City = "Medellín" }
        };

        context.Venues.AddRange(venues);
        await context.SaveChangesAsync();
    }
}
