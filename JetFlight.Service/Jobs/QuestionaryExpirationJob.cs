using JetFlight.IntegrationDataAccess;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class QuestionaryExpirationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(QuestionaryExpirationJob), JobConstants.LoyaltyGroup);
        public int QuestionaryId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var questionary = await integrationDb.Questionaries.FirstAsync(x => x.Id == QuestionaryId);

            if (questionary.Status == Shared.Models.Questionary.QuestionaryStatus.Activated)
            {
                questionary.Status = Shared.Models.Questionary.QuestionaryStatus.Archived;
                questionary.UpdatedAt = DateTime.UtcNow;

                await integrationDb.SaveChangesAsync();
            }
        }
    }
}
