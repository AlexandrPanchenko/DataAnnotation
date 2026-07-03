using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class LoyaltyActiveNotificationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(LoyaltyActiveNotificationJob), JobConstants.LoyaltyGroup);

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var jobSchedulerService = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            var now = DateTime.UtcNow;
            var maxStartDate = now.AddMinutes(45);

            var couponsToNotify = await integrationDb.Coupons
                .Where(x => !x.IsCardCoupon 
                    && x.Status == Shared.Models.Coupons.CouponStatus.Active
                    && x.StartDate >= now && x.StartDate <= maxStartDate)
                .ToListAsync();

            foreach (var coupon in couponsToNotify)
            {
                await jobSchedulerService.SetCouponActiveNotificationJobAsync(coupon.Id, coupon.StartDate, true);
            }

            var cardsToNotify = await integrationDb.AccumulationCards
                .Include(x => x.Coupons)
                .Where(x => x.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active
                    && x.Coupons.First().StartDate >= now && x.Coupons.First().StartDate <= maxStartDate)
                .ToListAsync();

            foreach (var card in cardsToNotify)
            {
                await jobSchedulerService.SetAccumulationCardActiveNotificationJobAsync(card.Id, card.Coupons.First().StartDate, true);
            }
        }
    }
}
