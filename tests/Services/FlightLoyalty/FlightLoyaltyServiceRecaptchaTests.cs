using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Users;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

public class FlightLoyaltyServiceRecaptchaTests
{
    private static PagingDTO InitialPaging => new() { Skip = 0, Take = 10 };

    [Fact]
    public async Task GetAllPromotionsClient_WebInitialLoad_ThrowsWhenTokenMissing()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("REC-1", "Recaptcha Promo Product");
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo", product)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Service.GetAllPromotionsClient(
                FlightLoyaltyTestDataBuilder.DefaultBranchId,
                RegistrationPlatform.Web,
                InitialPaging,
                token: null));
    }

    [Fact]
    public async Task GetAllPromotionsClient_WebWithSearch_SkipsRecaptchaValidation()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("REC-2", "Searchable Promo Product");
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Searchable Promo", product)));

        var result = await ctx.Service.GetAllPromotionsClient(
            FlightLoyaltyTestDataBuilder.DefaultBranchId,
            RegistrationPlatform.Web,
            InitialPaging,
            searchParam: "searchable",
            token: null);

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAllPromotionsClient_WebInitialLoad_SucceedsWithCachedToken()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("REC-3", "Cached Token Product");
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo", product)));

        const string token = "cached-recaptcha-token";
        ctx.MemoryCache.Set($"Recaptcha_{token}", true, TimeSpan.FromMinutes(2));

        var result = await ctx.Service.GetAllPromotionsClient(
            FlightLoyaltyTestDataBuilder.DefaultBranchId,
            RegistrationPlatform.Web,
            InitialPaging,
            token: token);

        Assert.Single(result.Items);
    }
}
