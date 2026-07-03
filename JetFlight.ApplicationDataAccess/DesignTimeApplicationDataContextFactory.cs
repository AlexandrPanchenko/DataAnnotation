using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JetFlight.ApplicationDataAccess;

public class DesignTimeApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        Environment.SetEnvironmentVariable(
            "DATABASE_URL",
            "postgres://postgres:postgres@localhost:5432/JetFlight_App_Fake");

        return new ApplicationDataContext();
    }
}
