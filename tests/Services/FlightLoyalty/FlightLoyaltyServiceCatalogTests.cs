using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Store;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

public class FlightLoyaltyServiceCatalogTests
{
    [Fact]
    public async Task GetAllPromotionTypes_ReturnsOnlyActive_WhenAllIsFalse()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();

        var result = await ctx.Service.GetAllPromotionTypes(all: false);

        Assert.Single(result);
        Assert.Equal("Active Type", result[0].Title);
    }

    [Fact]
    public async Task GetAllPromotionCategories_FiltersInactiveAndOrdersByPosition()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();

        var result = await ctx.Service.GetAllPromotionCategories(all: false);

        Assert.Single(result);
        Assert.Equal("CAT-ACTIVE", result[0].Code);
    }

    [Fact]
    public async Task GetAllPromotionTags_ReturnsOnlyActive_WhenAllIsFalse()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();

        var result = await ctx.Service.GetAllPromotionTags(all: false);

        Assert.Single(result);
        Assert.Equal("Active Tag", result[0].Title);
    }
}

public class FlightLoyaltyServiceDisplayRuleTests
{
    [Fact]
    public async Task GetDisplayRuleAsync_MapsAgeRuleFields()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();

        var result = await ctx.Service.GetDisplayRuleAsync(Branches.BirdJet);

        Assert.Equal(Branches.BirdJet, result.BranchId);
        Assert.Equal(3, result.RelevantCount);
        Assert.Equal(5, result.PerRuleCount);
        Assert.Equal(PromotionRulePeriod.Quarter, result.Period);
        var ageRule = Assert.IsType<PromotionDisplayAgeRuleDTO>(result.Rule);
        Assert.Equal(18, ageRule.Age.From);
        Assert.Equal(65, ageRule.Age.To);
    }

    [Fact]
    public async Task UpdateDisplayRuleAsync_SwitchesToLocationRuleAndPersistsStores()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();

        await ctx.Service.UpdateDisplayRuleAsync(new PromotionDisplayRuleDTO
        {
            BranchId = Branches.BirdJet,
            RelevantCount = 2,
            PerRuleCount = 4,
            Period = PromotionRulePeriod.HalfYear,
            Rule = new PromotionDisplayLocationRuleDTO
            {
                StoreIds = [1],
            },
        });

        var entity = await ctx.IntegrationContext.PromotionDisplayRules
            .Include(x => x.Stores)
            .FirstAsync(x => x.BranchId == (byte)Branches.BirdJet);

        Assert.Equal(PromotionDisplayRuleType.Location, entity.Type);
        Assert.Null(entity.AgeFrom);
        Assert.Null(entity.AgeTo);
        Assert.Equal(2, entity.RelevantCount);
        Assert.Equal(4, entity.PerRuleCount);
        Assert.Equal(PromotionRulePeriod.HalfYear, entity.Period);
        Assert.Single(entity.Stores);
        Assert.Equal(FlightLoyaltyTestDataBuilder.DefaultStoreNumber, entity.Stores[0].StoreCode);
    }

    [Fact]
    public async Task GetDisplayRulesAsync_ReturnsAllBranches()
    {
        using var ctx = new FlightLoyaltyServiceTestContext();
        await ctx.SeedAsync();

        var result = await ctx.Service.GetDisplayRulesAsync();

        Assert.Equal(Enum.GetValues<Branches>().Length, result.Count);
    }
}
