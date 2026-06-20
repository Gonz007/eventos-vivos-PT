using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Venue> Venues { get; }
    DbSet<Event> Events { get; }
    DbSet<Reservation> Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
