using JetFlight.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.AspNetCore;

namespace JetFlight.Service.Jobs
{
    public static class AddJobServiceExtension
    {
        public static IServiceCollection AddJobServices(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // Get SQL Server connection string from environment variable (same as IntegrationDataContext)
            var databaseUrl = Environment.GetEnvironmentVariable("MSSQL_DATABASE_URL");
            var useInMemoryJobStore = configuration?.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development"
                || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            string? connectionString = null;

            if (!string.IsNullOrEmpty(databaseUrl))
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');
                var server = uri.Host;
                var port = uri.Port;
                var userId = userInfo[0];
                var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
                var database = uri.AbsolutePath.TrimStart('/');

                connectionString = $"Data Source={server},{port};Initial Catalog={database};User Id={userId};Password={password};TrustServerCertificate=True";
            }
            else if (configuration != null)
            {
                // Fallback to configuration connection string if environment variable is not set
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            services.AddQuartz(q =>
            {
                // Use SQL Server job store if connection string is available, otherwise fall back to in-memory
                if (!string.IsNullOrEmpty(connectionString) && !useInMemoryJobStore)
                {
                    q.UsePersistentStore(store =>
                    {
                        store.UseSqlServer(sql =>
                        {
                            sql.ConnectionString = connectionString;
                            sql.TablePrefix = "QRTZ_";
                        });
                        
                        // Enable clustering - this ensures only one instance executes each job
                        // When clustering is enabled, Quartz will coordinate job execution across instances
                        store.UseClustering();
                    });
                    
                    // Configure serializer - Use JSON serializer (requires Quartz.Serialization.Json package)
                    // JSON serializer is recommended as it doesn't suffer from versioning issues
                    q.SetProperty("quartz.serializer.type", "json");
                    
                    // Cluster check-in interval (milliseconds) - how often instances check in
                    q.SetProperty("quartz.jobStore.clusterCheckinInterval", "20000");
                    
                    // Configure scheduler properties for clustering
                    q.SchedulerId = "AUTO"; // Auto-generate unique instance ID
                    q.SchedulerName = "JetFlightScheduler";
                }
                else
                {
                    // Fallback to in-memory store if no connection string (for development/testing)
                    q.UseInMemoryStore();
                    q.SchedulerId = "AUTO";
                    q.SchedulerName = "JetFlightScheduler";
                }
            });

            services.AddQuartzServer(q => q.WaitForJobsToComplete = true);
            services.AddSingleton<IJobSchedulerService, JobSchedulerService>();

            return services;
        }
    }
}
