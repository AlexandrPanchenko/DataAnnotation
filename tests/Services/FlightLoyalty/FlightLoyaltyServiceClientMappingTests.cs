using Xunit;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

public class FlightLoyaltyServiceClientMappingTests
{
    [Fact]
    public async Task GetPromotionByIdAsync_SingleProductCombo_UsesOfferAsTitle()
    {
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("COMBO-1", "Product Name Should Not Show");
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateComplexPromotion(
                1,
                title: "Combo Promo Title",
                offer: "Combo Offer Title",
                description: "Combo desc",
                product: product,
                storeCode: null)));

        var result = await ctx.Service.GetPromotionByIdAsync(FlightLoyaltyTestDataBuilder.DefaultBranchId, 1);

        Assert.NotNull(result);
        Assert.Equal("Combo Offer Title", result!.Promotion?.Title);
    }

    [Fact]
    public async Task GetPromotionByIdAsync_SingleProductCombo_DoesNotFallbackToProductImage()
    {
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("COMBO-2", "Product");
        product.ImagePath = "product-only.jpg";
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateComplexPromotion(
                2,
                title: "Combo",
                offer: "Offer",
                description: "Desc",
                product: product,
                storeCode: null)));

        var result = await ctx.Service.GetPromotionByIdAsync(FlightLoyaltyTestDataBuilder.DefaultBranchId, 2);

        Assert.NotNull(result);
        Assert.True(string.IsNullOrEmpty(result!.Promotion?.Image));
    }

    [Fact]
    public async Task GetPromotionByIdAsync_SingleProductCombo_PromotionsNameUsesPromotionTitle()
    {
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("COMBO-3", "Product Label");
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateComplexPromotion(
                3,
                title: "Combo Promo Title",
                offer: "Offer Text",
                description: "Desc",
                product: product,
                storeCode: null)));

        var result = await ctx.Service.GetPromotionByIdAsync(FlightLoyaltyTestDataBuilder.DefaultBranchId, 3);

        Assert.NotNull(result);
        Assert.Equal("Combo Promo Title", result!.Promotion?.PromotionsName);
    }

    [Fact]
    public async Task GetPromotionByIdAsync_RegularPromotion_UsesProductTitleAndImage()
    {
        var product = FlightLoyaltyTestDataBuilder.CreateProduct("REG-1", "Regular Product");
        product.ImagePath = "regular.jpg";
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync(async c =>
            await FlightLoyaltyTestDataBuilder.AddPromotionAsync(c, FlightLoyaltyTestDataBuilder.CreateRegularPromotion(4, "Fallback Title", product)));

        var result = await ctx.Service.GetPromotionByIdAsync(FlightLoyaltyTestDataBuilder.DefaultBranchId, 4);

        Assert.NotNull(result);
        Assert.Equal("Regular Product", result!.Promotion?.Title);
        Assert.Equal("regular.jpg", result.Promotion?.Image);
    }
}
