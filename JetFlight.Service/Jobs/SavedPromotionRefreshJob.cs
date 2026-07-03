using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs;

[DisallowConcurrentExecution]
public class SavedPromotionRefreshJob : IJob
{
    public static JobKey Key = new(nameof(SavedPromotionRefreshJob), JobConstants.LoyaltyGroup);

    public async Task Execute(IJobExecutionContext context)
    {
        var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

        if (serviceProvider == null)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var integrationDb = scope.ServiceProvider.GetRequiredService<IntegrationDataContext>();

        var nowUtc = DateTime.UtcNow;

        var savedPromotions = await integrationDb.SavedPromotions
            .Include(x => x.Promotion)
            .Where(x => x.Promotion != null
                        && !string.IsNullOrEmpty(x.Promotion.ProductCode)
                        && !string.IsNullOrEmpty(x.Promotion.StoreCode))
            .ToListAsync();

        if (!savedPromotions.Any())
        {
            return;
        }

        var distinctKeys = savedPromotions
            .Select(x => (ProductCode: x.Promotion.ProductCode!, StoreCode: x.Promotion.StoreCode!))
            .Distinct()
            .ToList();

        var productCodes = distinctKeys.Select(x => x.ProductCode).Distinct().ToList();
        var storeCodes = distinctKeys.Select(x => x.StoreCode).Distinct().ToList();

        var activePromotions = await integrationDb.Promotions
            .Where(p => !p.InActive
                        && p.StartedAt <= nowUtc
                        && p.ExpiredAt > nowUtc
                        && !string.IsNullOrEmpty(p.ProductCode)
                        && !string.IsNullOrEmpty(p.StoreCode)
                        && productCodes.Contains(p.ProductCode!)
                        && storeCodes.Contains(p.StoreCode!))
            .ToListAsync();

        if (!activePromotions.Any())
        {
            return;
        }

        var latestActivePromotionByKey = activePromotions
            .GroupBy(p => (p.ProductCode!, p.StoreCode!))
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(p => p.ExpiredAt)
                    .ThenByDescending(p => p.CreatedAt)
                    .First());

        var savedPromotionPairs = savedPromotions
            .Select(x => (x.CustomerId, x.PromotionId))
            .ToHashSet();

        var savedPromotionsToRemove = new List<SavedPromotion>();

        foreach (var savedPromotion in savedPromotions)
        {
            var key = (savedPromotion.Promotion.ProductCode!, savedPromotion.Promotion.StoreCode!);

            if (!latestActivePromotionByKey.TryGetValue(key, out var activePromotion))
            {
                continue;
            }

            if (savedPromotion.PromotionId == activePromotion.Id)
            {
                continue;
            }

            if (savedPromotionPairs.Contains((savedPromotion.CustomerId, activePromotion.Id)))
            {
                savedPromotionsToRemove.Add(savedPromotion);
                continue;
            }

            savedPromotionPairs.Remove((savedPromotion.CustomerId, savedPromotion.PromotionId));
            savedPromotion.PromotionId = activePromotion.Id;
            savedPromotion.UpdatedAt = nowUtc;
            savedPromotionPairs.Add((savedPromotion.CustomerId, savedPromotion.PromotionId));
        }

        if (savedPromotionsToRemove.Any())
        {
            integrationDb.SavedPromotions.RemoveRange(savedPromotionsToRemove);
        }

        await integrationDb.SaveChangesAsync();
    }
}

