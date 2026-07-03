using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Models.Promotion;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

public class FlightLoyaltyServiceSavedPromotionTests
{
    [Fact]
    public async Task AddSavedPromotion_ReturnsFalse_WhenCustomerIdIsNull()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo")));

        var result = await ctx.Service.AddSavedPromotion(new AddSavedPromotionDTO { PromotionId = 1 });

        Assert.False(result);
    }

    [Fact]
    public async Task AddSavedPromotion_ReturnsFalse_WhenPromotionDoesNotExist()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();
        ctx.UserContext.CustomerId = 42;

        var result = await ctx.Service.AddSavedPromotion(new AddSavedPromotionDTO { PromotionId = 999 });

        Assert.False(result);
    }

    [Fact]
    public async Task AddSavedPromotion_ReturnsFalse_WhenAlreadySaved()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo"));
            await FlightLoyaltyTestDataBuilder.AddSavedPromotionAsync(c, customerId: 42, promotionId: 1);
        });
        ctx.UserContext.CustomerId = 42;

        var result = await ctx.Service.AddSavedPromotion(new AddSavedPromotionDTO { PromotionId = 1 });

        Assert.False(result);
    }

    [Fact]
    public async Task AddSavedPromotion_SchedulesJobs_WhenPromotionStartsInFuture()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateFuturePromotion(10)));
        ctx.UserContext.CustomerId = 42;
        await FlightLoyaltyTestDataBuilder.EnsureCustomersAsync(ctx.IntegrationContext, 42);
        await ctx.IntegrationContext.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);

        var result = await ctx.Service.AddSavedPromotion(new AddSavedPromotionDTO { PromotionId = 10 });

        Assert.True(result);
        ctx.JobSchedulerMock.Verify(
            j => j.SetSavedPromotionStartNotificationJobAsync(10, It.IsAny<DateTime>(), true),
            Times.Once);
        ctx.JobSchedulerMock.Verify(
            j => j.SetSavedPromotionDayBeforeExpirationJobAsync(10, It.IsAny<DateTime>(), true),
            Times.Once);
    }

    [Fact]
    public async Task DeleteSavedPromotion_ReturnsFalse_WhenNotFound()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();
        ctx.UserContext.CustomerId = 42;

        var result = await ctx.Service.DeleteSavedPromotion(1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteSavedPromotion_RemovesRowAndJobs_WhenLastCustomerUnsaves()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo"));
            await FlightLoyaltyTestDataBuilder.AddSavedPromotionAsync(c, customerId: 42, promotionId: 1);
        });
        ctx.UserContext.CustomerId = 42;

        var result = await ctx.Service.DeleteSavedPromotion(1);

        Assert.True(result);
        Assert.False(await ctx.IntegrationContext.SavedPromotions.AnyAsync());
        ctx.JobSchedulerMock.Verify(j => j.RemoveSavedPromotionStartNotificationJobAsync(1), Times.Once);
        ctx.JobSchedulerMock.Verify(j => j.RemoveSavedPromotionDayBeforeExpirationJobAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteSavedPromotion_DoesNotRemoveJobs_WhenAnotherCustomerStillSaved()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo"));
            await FlightLoyaltyTestDataBuilder.AddSavedPromotionAsync(c, customerId: 42, promotionId: 1);
            await FlightLoyaltyTestDataBuilder.AddSavedPromotionAsync(c, customerId: 43, promotionId: 1);
        });
        ctx.UserContext.CustomerId = 42;

        var result = await ctx.Service.DeleteSavedPromotion(1);

        Assert.True(result);
        Assert.Single(ctx.IntegrationContext.SavedPromotions);
        ctx.JobSchedulerMock.Verify(j => j.RemoveSavedPromotionStartNotificationJobAsync(It.IsAny<int>()), Times.Never);
        ctx.JobSchedulerMock.Verify(j => j.RemoveSavedPromotionDayBeforeExpirationJobAsync(It.IsAny<int>()), Times.Never);
    }
}
