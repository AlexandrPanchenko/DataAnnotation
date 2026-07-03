using System;
using JetFlight.Shared.UserContext;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JetFlight.IntegrationDataAccess;

public class DesignTimeIntegrationDataContextFactory : IDesignTimeDbContextFactory<IntegrationDataContext>
{
    public IntegrationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IntegrationDataContext>();

        // Використовуємо ту ж логіку, що й у OnConfiguring:
        var databaseUrl = Environment.GetEnvironmentVariable("MSSQL_DATABASE_URL");

        if (!string.IsNullOrEmpty(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');

            var server = uri.Host;
            var port = uri.Port;
            var userId = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');

            optionsBuilder.UseSqlServer(
                $"Data Source={server},{port};Initial Catalog={database};User Id={userId};Password={password};TrustServerCertificate=True"
            );
        }
        else
        {
            // Fallback для генерації міграцій локально
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=JetFlightIntegration_Fake;Trusted_Connection=True;TrustServerCertificate=True");
        }

        // Для design-time достатньо заглушки IBus, оскільки міграції не відправляють повідомлення
        IBus bus = null!;
        var userContext = new UserContext();

        return new IntegrationDataContext(optionsBuilder.Options, bus, userContext);
    }
}

