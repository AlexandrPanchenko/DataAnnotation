using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs;

[DisallowConcurrentExecution]
public class PageAutoPublishJob : IJob
{
    public static readonly JobKey Key = new(nameof(PageAutoPublishJob), JobConstants.ContentGroup);

    public async Task Execute(IJobExecutionContext context)
    {
        var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

        if (serviceProvider == null)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var pageManagementService = scope.ServiceProvider.GetRequiredService<IPageManagementService>();

        await pageManagementService.UpdateAllPageStatus();
    }
}

