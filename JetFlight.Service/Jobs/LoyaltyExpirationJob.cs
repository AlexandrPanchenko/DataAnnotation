using JetFlight.IntegrationDataAccess;
using JetFlight.Service.Extensions;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class LoyaltyExpirationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(LoyaltyExpirationJob), JobConstants.LoyaltyGroup);

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var jobSchedulerService = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            await integrationDb.Coupons
                .Include(x => x.Questionaries)
                .Where(x => x.ExpirationDate <= DateTime.UtcNow && x.Status == Shared.Models.Coupons.CouponStatus.Active && !x.IsCardCoupon)
                .ForEachAsync(x =>
                {
                    x.Status = Shared.Models.Coupons.CouponStatus.Archived;
                    x.UpdatedAt = DateTime.UtcNow;

                    var questionariesToChange = x.Questionaries.Where(q => q.CouponId == x.Id && q.Status == Shared.Models.Questionary.QuestionaryStatus.Activated);

                    foreach (var questionary in questionariesToChange)
                    {
                        questionary.Status = Shared.Models.Questionary.QuestionaryStatus.Archived;
                        questionary.UpdatedAt = DateTime.UtcNow;
                    };
                });

            await integrationDb.AccumulationCards
                .Include(x => x.Coupons)
                .Where(x => x.Coupons.Any(c => c.ExpirationDate <= DateTime.UtcNow) && x.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active)
                .ForEachAsync(x =>
                {
                    x.Status = Shared.Models.AccumulationCard.AccumulationCardStatus.Archived;
                    x.UpdatedAt = DateTime.UtcNow;
                    x.Coupons.ForEach(x =>
                    {
                        x.Status = Shared.Models.Coupons.CouponStatus.Archived;
                        x.UpdatedAt = DateTime.UtcNow;
                    });
                });

            await integrationDb.Questionaries.Where(x => x.ExpirationDate <= DateTime.UtcNow && x.Status == Shared.Models.Questionary.QuestionaryStatus.Activated)
                .ForEachAsync(x =>
                {
                    x.Status = Shared.Models.Questionary.QuestionaryStatus.Archived;
                    x.UpdatedAt = DateTime.UtcNow;
                });

            await integrationDb.SaveChangesAsync();

            var dateTime = DateTime.UtcNow.AddHours(JobConstants.LoyaltyExpirationJobIntervalHours).AddMinutes(JobConstants.LoyaltyExpirationJobDelayMinutes);

            var coupons = await integrationDb.Coupons
                .Where(x => x.Status == Shared.Models.Coupons.CouponStatus.Active && !x.IsCardCoupon && x.ExpirationDate < dateTime)
                .ToListAsync();
            
            foreach (var coupon in coupons)
            {
                await jobSchedulerService.SetCouponExpirationJobAsync(coupon.Id, coupon.ExpirationDate, true);
            }

            var accumulationCards = await integrationDb.AccumulationCards
                .Where(x => x.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active 
                    && x.Coupons.Any(x => x.ExpirationDate <= dateTime))
                .ToListAsync();

            foreach (var card in accumulationCards)
            {
                await jobSchedulerService.SetAccumulationExpirationJobAsync(card.Id, card.Coupons.First().ExpirationDate, true);
            }

            // Schedule expiration notifications for saved promotions (at 3pm on expiration day)
            var promotions = await integrationDb.SavedPromotions
                .Include(x => x.Promotion)
                .Where(x => x.Promotion.ExpiredAt < dateTime)
                .Select(x => new { x.PromotionId, x.Promotion.ExpiredAt })
                .Distinct()
                .ToListAsync();

            foreach (var promotion in promotions)
            {
                // Schedule notification if it hasn't passed 3pm on expiration day yet
                // Ignore time from database, use only date
                var expirationDateOnly = promotion.ExpiredAt.Date;
                // Convert to local timezone, set 3pm, then convert back to UTC (same logic as SetSavedPromotionDayBeforeExpirationJobAsync)
                var expirationLocal = expirationDateOnly.FromUtcToTimezone(TimeZoneConstants.UATimezone);
                var expirationDay3PMLocal = new DateTime(
                    expirationLocal.Year,
                    expirationLocal.Month,
                    expirationLocal.Day,
                    15, 0, 0,
                    DateTimeKind.Unspecified);
                var expirationDay3PM = expirationDay3PMLocal.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                
                if (expirationDay3PM > DateTime.UtcNow)
                {
                    await jobSchedulerService.SetSavedPromotionDayBeforeExpirationJobAsync(promotion.PromotionId, expirationDateOnly, true);
                }
            }

            var questionaries = await integrationDb.Questionaries
                .Where(x => x.Status == Shared.Models.Questionary.QuestionaryStatus.Activated && x.ExpirationDate <= dateTime)
                .ToListAsync();

            foreach (var questionary in questionaries)
            {
                await jobSchedulerService.SetQuestionaryExpirationJobAsync(questionary.Id, questionary.ExpirationDate, true);
            }
        }
    }
}
