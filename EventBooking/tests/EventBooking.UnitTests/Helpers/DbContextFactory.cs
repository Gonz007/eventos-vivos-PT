using EventBooking.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.UnitTests.Helpers;

/// <summary>
/// Creates an in-memory SQLite database for unit tests.
/// Each call with a unique name gets its own isolated database.
/// </summary>
public static class DbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        // SQLite in-memory with a named connection so the schema persists
        // for the life of the connection (needed for multiple operations).
        var connectionString = $"Data Source={dbName ?? Guid.NewGuid().ToString()};Mode=Memory;Cache=Shared";
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
