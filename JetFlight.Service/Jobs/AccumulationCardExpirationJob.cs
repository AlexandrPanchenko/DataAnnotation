using JetFlight.IntegrationDataAccess;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class AccumulationCardExpirationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(AccumulationCardExpirationJob), JobConstants.LoyaltyGroup);

        public int AccumulationCardId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var entity = await integrationDb.AccumulationCards
                .Include(x => x.Coupons)
                .FirstAsync(x => x.Id == AccumulationCardId);

            if (entity.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active)
            {
                entity.Status = Shared.Models.AccumulationCard.AccumulationCardStatus.Archived;
                entity.UpdatedAt = DateTime.UtcNow;

                foreach (var coupon in entity.Coupons)
                {
                    coupon.Status = Shared.Models.Coupons.CouponStatus.Archived;
                    coupon.UpdatedAt = DateTime.UtcNow;
                }

                await integrationDb.SaveChangesAsync();
            }
        }
    }
}
