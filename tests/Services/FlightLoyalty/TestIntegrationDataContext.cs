using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.UserContext;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

/// <summary>
/// SQLite-backed IntegrationDataContext for unit tests.
/// Skips SQL Server connection override and removes trigger annotations unsupported by SQLite.
/// </summary>
internal sealed class TestIntegrationDataContext : IntegrationDataContext
{
    public TestIntegrationDataContext(SqliteConnection connection, IBus bus, IUserContext userContext)
        : base(CreateOptions(connection), bus, userContext)
    {
    }

    private static DbContextOptions<IntegrationDataContext> CreateOptions(SqliteConnection connection)
    {
        return new DbContextOptionsBuilder<IntegrationDataContext>()
            .UseSqlite(connection)
            .Options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Do not call base — avoids MSSQL_DATABASE_URL / LocalDB override.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Promotion>().ToTable("Promotions");
        modelBuilder.Entity<Product>().ToTable("Products");
    }
}
