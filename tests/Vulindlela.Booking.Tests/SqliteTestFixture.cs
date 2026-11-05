
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;

namespace Vulindlela.Booking.Tests;

public sealed class SqliteTestFixture : IDisposable
{
    private readonly SqliteConnection _keepAlive;

    public DbContextOptions<BookingContext> Options { get; }

    public SqliteTestFixture()
    {
        // Give each fixture its own database.
        string databaseName =
            "vulindlela-tests-" + Guid.NewGuid().ToString("N");

        string connectionString =
            $"Data Source={databaseName};Mode=Memory;Cache=Shared";

        // Keep one connection open so the database survives
        // while individual DbContexts open and close.
        _keepAlive = new SqliteConnection(connectionString);
        _keepAlive.Open();

        // Pass the connection STRING, not the connection object.
        Options = new DbContextOptionsBuilder<BookingContext>()
            .UseSqlite(connectionString)
            .Options;

        using var context = CreateContext();

        // Create real SQLite tables and unique indexes.
        context.Database.EnsureCreated();
    }

    public BookingContext CreateContext()
    {
        return new BookingContext(Options);
    }

    public void Dispose()
    {
        _keepAlive.Dispose();
    }
}
