using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.FlightLoyalty;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Store;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

internal static class FlightLoyaltyTestDataBuilder
{
    public const string DefaultStoreNumber = "M049";
    public const string DefaultCategoryCode = "CAT-ACTIVE";
    public const byte DefaultBranchId = (byte)Branches.BirdJet;

    public static IReadOnlyList<Store> DefaultStores { get; } =
    [
        new Store
        {
            Id = 1,
            Number = DefaultStoreNumber,
            BranchId = DefaultBranchId,
            Title = "Test Store",
            Address = "Test Address",
            isActive = true,
            CityId = 1,
        }
    ];

    public static async Task SeedCatalogAsync(IntegrationDataContext context)
    {
        if (!await context.PromotionsType.AnyAsync())
        {
            await context.PromotionsType.AddRangeAsync(
                new PromotionType
                {
                    Id = 1,
                    NavisionId = "TYPE-1",
                    Title = "Active Type",
                    IsActive = true,
                    Position = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                new PromotionType
                {
                    Id = 2,
                    NavisionId = "TYPE-2",
                    Title = "Inactive Type",
                    IsActive = false,
                    Position = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
        }

        if (!await context.WebProductCategories.AnyAsync())
        {
            await context.WebProductCategories.AddRangeAsync(
                new WebProductCategory
                {
                    Code = DefaultCategoryCode,
                    Title = "Active Category",
                    Image = "cat.png",
                    IsActive = true,
                    Position = 1,
                },
                new WebProductCategory
                {
                    Code = "CAT-INACTIVE",
                    Title = "Inactive Category",
                    Image = "cat2.png",
                    IsActive = false,
                    Position = 2,
                });
        }

        if (!await context.ProductsTags.AnyAsync())
        {
            await context.ProductsTags.AddRangeAsync(
                new ProductsTag
                {
                    Id = 1,
                    Title = "Active Tag",
                    Code = "TAG-1",
                    Icon = "tag.png",
                    IsActive = true,
                    Position = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                new ProductsTag
                {
                    Id = 2,
                    Title = "Inactive Tag",
                    Code = "TAG-2",
                    Icon = "tag2.png",
                    IsActive = false,
                    Position = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
        }

        await EnsureProductHierarchyAsync(context);
    }

    public static async Task EnsureCustomersAsync(IntegrationDataContext context, params int[] customerIds)
    {
        foreach (var customerId in customerIds)
        {
            if (await context.Customers.AnyAsync(c => c.Id == customerId))
            {
                continue;
            }

            await context.Customers.AddAsync(new Customer
            {
                Id = customerId,
                PhoneNumber = $"+380{customerId:D9}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }
    }

    public static async Task AddSavedPromotionAsync(
        IntegrationDataContext context,
        int customerId,
        int promotionId)
    {
        await EnsureCustomersAsync(context, customerId);
        context.SavedPromotions.Add(new SavedPromotion
        {
            CustomerId = customerId,
            PromotionId = promotionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    public static async Task EnsureProductHierarchyAsync(IntegrationDataContext context)
    {
        if (context.ProductBrands.Local.Count > 0 || await context.ProductBrands.AnyAsync())
        {
            return;
        }

        await context.ProductDivisions.AddAsync(new ProductDivision { Code = "DIV-1", Title = "Division" });
        await context.ProductSegments.AddAsync(new ProductSegment
        {
            Code = "SEG-1",
            Title = "Segment",
            DivisionCode = "DIV-1",
        });
        await context.ProductCategories.AddAsync(new ProductCategory
        {
            Code = "PCAT-1",
            Title = "Product Category",
            Image = string.Empty,
            SegmentCode = "SEG-1",
        });
        await context.ProductManufacturers.AddAsync(new ProductManufacturer { Code = "MFR-1", Title = "Manufacturer" });
        await context.ProductFamilies.AddAsync(new ProductFamily
        {
            Code = "FAM-1",
            Title = "Family",
            CategoryCode = "PCAT-1",
        });
        await context.ProductBrands.AddAsync(new ProductBrand
        {
            Code = "BRAND-1",
            Title = "Brand",
            ManufacturerCode = "MFR-1",
        });
    }

    public static async Task SeedDisplayRulesAsync(IntegrationDataContext context)
    {
        foreach (Branches branch in Enum.GetValues<Branches>())
        {
            if (await context.PromotionDisplayRules.AnyAsync(x => x.BranchId == (byte)branch))
            {
                continue;
            }

            await context.PromotionDisplayRules.AddAsync(new PromotionDisplayRule
            {
                BranchId = (byte)branch,
                AgeFrom = 18,
                AgeTo = 65,
                RelevantCount = 3,
                PerRuleCount = 5,
                Period = PromotionRulePeriod.Quarter,
                Type = PromotionDisplayRuleType.Age,
                Stores = new List<PromotionDisplayRuleToStore>(),
                TypesOfActivity = new List<PromotionDisplayRuleToActivityType>(),
            });
        }
    }

    public static Product CreateProduct(string code, string title, int? tagId = null)
    {
        var product = new Product
        {
            Code = code,
            Title = title,
            ImagePath = $"{code}-image.jpg",
            FamilyCode = "FAM-1",
            BrandCode = "BRAND-1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ProductTags = new List<ProductTag>(),
        };

        if (tagId.HasValue)
        {
            product.ProductTags.Add(new ProductTag
            {
                TagId = tagId.Value,
            });
        }

        return product;
    }

    public static (Product Product, Ticket Ticket) CreateTicket(
        string flightNumber,
        string routeLabel,
        int destinationAirportId,
        int? tagId = null)
    {
        var product = CreateProduct(flightNumber, routeLabel, tagId);
        var ticket = new Ticket
        {
            DestinationAirportId = destinationAirportId,
            FlightNumber = flightNumber,
            PassengerName = routeLabel,
        };

        return (product, ticket);
    }

    public static Promotion CreateRegularPromotion(
        int id,
        string title,
        Product? product = null,
        string? storeCode = DefaultStoreNumber,
        string? promotionTypeNavisionId = null,
        bool inactive = false,
        DateTime? expiredAt = null,
        string? eligibleAirportIds = null)
    {
        var now = DateTime.UtcNow;
        return new Promotion
        {
            Id = id,
            Title = title,
            Description = $"{title} description",
            Offer = null,
            Price = 100m,
            PromoPrice = 80m,
            Image = string.Empty,
            StoreCode = storeCode,
            EligibleAirportIds = eligibleAirportIds,
            ProductCode = product?.Code,
            Product = product,
            PromotionTypeId = promotionTypeNavisionId,
            StartedAt = now.AddDays(-1),
            ExpiredAt = expiredAt ?? now.AddDays(30),
            InActive = inactive,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now,
            IsComplexPromotion = false,
            ItemUnit = ItemUnit.Items,
        };
    }

    public static Promotion CreateComplexPromotion(
        int id,
        string title,
        string offer,
        string description,
        Product? product = null,
        string? storeCode = null,
        string? eligibleAirportIds = null)
    {
        var now = DateTime.UtcNow;
        return new Promotion
        {
            Id = id,
            Title = title,
            Description = description,
            Offer = offer,
            Price = 0m,
            PromoPrice = 0m,
            Image = string.Empty,
            StoreCode = storeCode,
            EligibleAirportIds = eligibleAirportIds,
            ProductCode = product?.Code,
            Product = product,
            StartedAt = now.AddDays(-1),
            ExpiredAt = now.AddDays(30),
            InActive = false,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now,
            IsComplexPromotion = true,
            ItemUnit = ItemUnit.Items,
        };
    }

    public static Promotion CreateFuturePromotion(int id)
    {
        var now = DateTime.UtcNow;
        return new Promotion
        {
            Id = id,
            Title = "Future Promo",
            Description = "Starts in the future",
            Price = 50m,
            PromoPrice = 40m,
            Image = string.Empty,
            StoreCode = DefaultStoreNumber,
            StartedAt = now.AddDays(2),
            ExpiredAt = now.AddDays(10),
            InActive = false,
            CreatedAt = now,
            UpdatedAt = now,
            IsComplexPromotion = false,
            ItemUnit = ItemUnit.Items,
        };
    }

    public static async Task AddPromotionAsync(
        IntegrationDataContext context,
        Promotion promotion,
        string categoryCode = DefaultCategoryCode)
    {
        if (promotion.Product != null)
        {
            await AddProductsAsync(context, promotion.Product);
        }

        promotion.WebProductCategory = await context.WebProductCategories.FirstAsync(c => c.Code == categoryCode);
        context.Promotions.Add(promotion);
    }

    public static async Task AddProductsAsync(IntegrationDataContext context, params Product[] products)
    {
        await EnsureProductHierarchyAsync(context);

        foreach (var product in products)
        {
            foreach (var tag in product.ProductTags)
            {
                tag.ProductsTag = await context.ProductsTags.FindAsync(tag.TagId)
                    ?? throw new InvalidOperationException($"Tag {tag.TagId} not found");
            }
        }

        await context.Products.AddRangeAsync(products);
    }
}
