using JetFlight.IntegrationDataAccess;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class CouponExpirationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(CouponExpirationJob), JobConstants.LoyaltyGroup);

        public int CouponId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var jobSchedulerService = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            var entity = await integrationDb.Coupons
                .Include(x => x.Questionaries)
                .FirstAsync(x => x.Id == CouponId);

            if (entity.Status == Shared.Models.Coupons.CouponStatus.Active)
            {
                entity.Status = Shared.Models.Coupons.CouponStatus.Archived;
                entity.UpdatedAt = DateTime.UtcNow;

                var questionariesToArchive = entity.Questionaries.Where(x => x.Status == Shared.Models.Questionary.QuestionaryStatus.Activated).ToList();
                foreach (var questionary in questionariesToArchive)
                {
                    questionary.Status = Shared.Models.Questionary.QuestionaryStatus.Archived;
                    questionary.UpdatedAt = DateTime.UtcNow;
                };

                await integrationDb.SaveChangesAsync();

                foreach (var questionary in questionariesToArchive)
                {
                    await jobSchedulerService.RemoveQuestionaryExpirationJobAsync(questionary.Id);
                }
            }
        }
    }
}
