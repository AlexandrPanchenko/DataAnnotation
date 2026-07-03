using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using Xunit;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

public class FlightLoyaltyServiceSearchTests
{
    private static PagingDTO DefaultPaging => new() { Skip = 0, Take = 50 };

    [Fact]
    public async Task GetAllPromotionsAdmin_MatchesPromotionTitle_CaseInsensitive()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateComplexPromotion(
                1, "Organic Combo", offer: "Different Offer", description: "Desc"));
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(2, "Other Promo"));
        });

        var result = await ctx.Service.GetAllPromotionsAdmin(DefaultPaging, searchParam: "organic");

        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].Promotion?.Id);
    }

    [Fact]
    public async Task GetAllPromotionsAdmin_MatchesProductTitle()
    {
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("P-1", "Молоко Organic");
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Promo A", product));
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(2, "Promo B"));
        });

        var result = await ctx.Service.GetAllPromotionsAdmin(DefaultPaging, searchParam: "organic");

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAllPromotionsAdmin_MatchesOfferAndDescription_OnComplexPromotions()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateComplexPromotion(
                1, "Combo Title", offer: "Buy Two", description: "Secret description"));
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(2, "Regular"));
        });

        var byOffer = await ctx.Service.GetAllPromotionsAdmin(DefaultPaging, searchParam: "buy");
        var byDescription = await ctx.Service.GetAllPromotionsAdmin(DefaultPaging, searchParam: "secret");

        Assert.Single(byOffer.Items);
        Assert.Single(byDescription.Items);
    }

    [Fact]
    public async Task GetAllSavedPromotions_MatchesComplexOffer()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateComplexPromotion(
                1, "Combo", offer: "Mega Offer", description: "Desc", storeCode: FlightLoyaltyTestDataBuilder.DefaultStoreNumber));
            await FlightLoyaltyTestDataBuilder.AddSavedPromotionAsync(c, customerId: 7, promotionId: 1);
        });
        ctx.UserContext.CustomerId = 7;
        ctx.UserContext.BranchId = Branches.BirdJet;

        var result = await ctx.Service.GetAllSavedPromotions(DefaultPaging, searchParam: "mega");

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAllPromotionsClient_ExcludesExpiredAndInactivePromotions()
    {
        var activeProduct = FlightLoyaltyTestDataBuilder.CreateProduct("P-ACTIVE", "Active Product");
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Active", activeProduct));
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(2, "Inactive", inactive: true));
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(
                3, "Expired", expiredAt: DateTime.UtcNow.AddDays(-1)));
        });

        var result = await ctx.Service.GetAllPromotionsClient(
            FlightLoyaltyTestDataBuilder.DefaultBranchId,
            RegistrationPlatform.App,
            DefaultPaging);

        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].Promotion?.Id);
    }

    [Fact]
    public async Task GetAllPromotionsClient_FiltersByTagAndCategory()
    {
        var taggedProduct = FlightLoyaltyTestDataBuilder.CreateProduct("P-TAG", "Tagged Product", tagId: 1);
        var otherProduct = FlightLoyaltyTestDataBuilder.CreateProduct("P-OTHER", "Other Product");
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
        {
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(1, "Tagged Promo", taggedProduct));
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(
                c,
                FlightLoyaltyTestDataBuilder.CreateRegularPromotion(2, "Other Promo", otherProduct),
                categoryCode: "CAT-INACTIVE");
        });

        var byTag = await ctx.Service.GetAllPromotionsClient(
            FlightLoyaltyTestDataBuilder.DefaultBranchId,
            RegistrationPlatform.App,
            DefaultPaging,
            promotionTagIds: "1");

        var byCategory = await ctx.Service.GetAllPromotionsClient(
            FlightLoyaltyTestDataBuilder.DefaultBranchId,
            RegistrationPlatform.App,
            DefaultPaging,
            categoryCode: FlightLoyaltyTestDataBuilder.DefaultCategoryCode);

        Assert.Single(byTag.Items);
        Assert.Single(byCategory.Items);
        Assert.Equal(1, byTag.Items[0].Promotion?.Id);
        Assert.Equal(1, byCategory.Items[0].Promotion?.Id);
    }
}
