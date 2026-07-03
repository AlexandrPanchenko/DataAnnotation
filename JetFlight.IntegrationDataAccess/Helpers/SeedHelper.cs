using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.IntegrationDataAccess.Helpers
{
    public static class SeedHelper
    {
        private const int SeedPromotionId = 9001;
        private const string SeedPromotionTypeNavisionId = "SEED-TYPE-1";
        private const string SeedCategoryCode = "SEED-CAT-1";

        public static async Task Seed(this IntegrationDataContext dataContext, PersonalDataSelectOptions personalDataSelectOptions)
        {
            await SeedQuestionaries(dataContext, personalDataSelectOptions);
            await SeedPromotionDisplayRules(dataContext);
            await SeedPromotions(dataContext);
            await dataContext.SaveChangesAsync();
        }

        private static async Task SeedQuestionaries(IntegrationDataContext dataContext, PersonalDataSelectOptions personalDataSelectOptions)
        {
            var personalDataQuestionary = new Questionary
            {
                IsLocked = true,
                Image = new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
                {
                    Path = $"{StorageConstants.AppPath}/{PersonalDataQuestionaryConstants.QuestionaryImageName}"
                }.ToString(),
                Alt = "",
                ExpirationDate = DateTime.MaxValue.ToUniversalTime(),
                ActiveDaysAfterComplete = 0,
                BonusReward = 30,
                Name = PersonalDataQuestionaryConstants.Name,
                Status = Shared.Models.Questionary.QuestionaryStatus.Activated,
                CreatedAt = DateTime.UtcNow,
                BranchId = null,
                Fields = new List<QuestionaryField>
                {
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.DateOfBirthField,
                        Position = 1,
                        Type = Shared.Models.Questionary.QuestionaryItemType.DateTime,
                        Validation = "18+",
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.SexField,
                        Position = 2,
                        Type = Shared.Models.Questionary.QuestionaryItemType.Select,
                        Options = new List<QuestionarySelectOption>
                        {
                            new QuestionarySelectOption
                            {
                                Key = PersonalDataQuestionaryConstants.SexFieldManOption,
                                Value = PersonalDataQuestionaryConstants.SexFieldManOption,
                            },
                            new QuestionarySelectOption
                            {
                                Key = PersonalDataQuestionaryConstants.SexFieldWomanOption,
                                Value = PersonalDataQuestionaryConstants.SexFieldWomanOption,
                            },
                        }
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.CityField,
                        Position = 3,
                        Type = Shared.Models.Questionary.QuestionaryItemType.Select,
                        Options = personalDataSelectOptions.Cities.Select(x => new QuestionarySelectOption
                        {
                            Key = x.Key,
                            Value = x.Value,
                        }).ToList(),
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.HomeAirportField,
                        Position = 4,
                        Type = Shared.Models.Questionary.QuestionaryItemType.Select,
                        Options = personalDataSelectOptions.AirportHubs.Select(x => new QuestionarySelectOption
                        {
                            Key = x.Key,
                            Value = x.Value,
                        }).ToList(),
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.TypeOfActivityField,
                        Position = 5,
                        Type = Shared.Models.Questionary.QuestionaryItemType.Select,
                        Options = new List<QuestionarySelectOption>
                        {
                            new QuestionarySelectOption
                            {
                                Key = "Студент",
                                Value = "Студент",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Працюю",
                                Value = "Працюю",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Не працюю",
                                Value = "Не працюю",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Пенсіонер",
                                Value = "Пенсіонер",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Інше",
                                Value = "Інше",
                            },
                        }
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.WhereFindOutField,
                        Position = 6,
                        Type = Shared.Models.Questionary.QuestionaryItemType.Select,
                        Options = new List<QuestionarySelectOption>
                        {
                            new QuestionarySelectOption
                            {
                                Key = "Mobile app",
                                Value = "Mobile app",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Airport kiosk",
                                Value = "Airport kiosk",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Facebook",
                                Value = "Facebook",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Instagram",
                                Value = "Instagram",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "TikTok",
                                Value = "TikTok",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "З реклами",
                                Value = "З реклами",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Від знайомих",
                                Value = "Від знайомих",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "Інше",
                                Value = "Інше",
                            },
                        }
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.NumberOfChildrenField,
                        Position = 7,
                        Type = Shared.Models.Questionary.QuestionaryItemType.Select,
                        Options = new List<QuestionarySelectOption>
                        {
                            new QuestionarySelectOption
                            {
                                Key = "0",
                                Value = "0",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "1",
                                Value = "1",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "2",
                                Value = "2",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "3",
                                Value = "3",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "4",
                                Value = "4",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "5",
                                Value = "5",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "6",
                                Value = "6",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "7",
                                Value = "7",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "8",
                                Value = "8",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "9",
                                Value = "9",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "10",
                                Value = "10",
                            },
                            new QuestionarySelectOption
                            {
                                Key = "10+",
                                Value = "10+",
                            },
                        }
                    },
                    new QuestionaryField
                    {
                        IsRequired = true,
                        Name = PersonalDataQuestionaryConstants.EmailField,
                        Position = 8,
                        Type = Shared.Models.Questionary.QuestionaryItemType.String,
                        Validation = Shared.Constants.RegexConstants.Email,
                    },
                }
            };

            var existingPersonalDataQuestionary = await dataContext.Questionaries
                .Include(x => x.Fields)
                .ThenInclude(x => x.Options)
                .FirstOrDefaultAsync(x => x.Name == PersonalDataQuestionaryConstants.Name && x.IsLocked);

            if (existingPersonalDataQuestionary == null)
            {
                await dataContext.Questionaries.AddAsync(personalDataQuestionary);
            }
            else
            {
                existingPersonalDataQuestionary.Image = personalDataQuestionary.Image;

                foreach (var field in personalDataQuestionary.Fields)
                {
                    var existingField = existingPersonalDataQuestionary.Fields.FirstOrDefault(x => x.Name == field.Name);
                    if (existingField == null)
                    {
                        existingPersonalDataQuestionary.Fields.Add(field);
                    }
                    else
                    {
                        existingField.Position = field.Position;
                        existingField.Type = field.Type;
                        existingField.Validation = field.Validation;

                        if (field.Type == Shared.Models.Questionary.QuestionaryItemType.Select || field.Type == Shared.Models.Questionary.QuestionaryItemType.Multiselect)
                        {
                            // Add or update options (don't remove existing options to avoid FK constraint issues)
                            // Old options will be handled by SQL migration script
                            foreach (var option in field.Options)
                            {
                                var existingOption = existingField.Options.FirstOrDefault(x => x.Key == option.Key);
                                if (existingOption == null)
                                {
                                    existingField.Options.Add(option);
                                }
                                else
                                {
                                    existingOption.Value = option.Value;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task SeedPromotionDisplayRules(IntegrationDataContext dataContext)
        {
            var branchIds = Enum.GetValues<Branches>();

            foreach (var branchId in branchIds)
            {
                if (!await dataContext.PromotionDisplayRules.AnyAsync(x => x.BranchId == (byte)branchId))
                {
                    await dataContext.PromotionDisplayRules.AddAsync(new PromotionDisplayRule
                    {
                        BranchId = (byte)branchId,
                        AgeFrom = 0,
                        AgeTo = 100,
                        Type = Shared.Models.Promotion.PromotionDisplayRuleType.Age,
                    });
                }
            }
        }

        private static async Task SeedPromotions(IntegrationDataContext dataContext)
        {
            if (await dataContext.Promotions.AnyAsync(p => p.Id == SeedPromotionId))
            {
                return;
            }

            var now = DateTime.UtcNow;

            if (!await dataContext.PromotionsType.AnyAsync(x => x.NavisionId == SeedPromotionTypeNavisionId))
            {
                await dataContext.PromotionsType.AddAsync(new PromotionType
                {
                    Id = 9001,
                    NavisionId = SeedPromotionTypeNavisionId,
                    Title = "Weekly deals",
                    IsActive = true,
                    Position = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    PromotionTypeBranches = Enum.GetValues<Branches>()
                        .Select(branch => new PromotionTypeBranch
                        {
                            BranchId = (int)branch,
                        })
                        .ToList(),
                });
            }

            if (!await dataContext.WebProductCategories.AnyAsync(x => x.Code == SeedCategoryCode))
            {
                await dataContext.WebProductCategories.AddRangeAsync(
                    new WebProductCategory
                    {
                        Code = SeedCategoryCode,
                        Title = "Groceries",
                        Image = "seed-groceries.png",
                        IsActive = true,
                        Position = 1,
                    },
                    new WebProductCategory
                    {
                        Code = "SEED-CAT-2",
                        Title = "Household",
                        Image = "seed-household.png",
                        IsActive = true,
                        Position = 2,
                    });
            }

            if (!await dataContext.ProductsTags.AnyAsync(x => x.Code == "SEED-TAG-1"))
            {
                await dataContext.ProductsTags.AddAsync(new ProductsTag
                {
                    Id = 9001,
                    Title = "Organic",
                    Code = "SEED-TAG-1",
                    Icon = "seed-tag.png",
                    IsActive = true,
                    Position = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }

            await EnsureSeedProductHierarchyAsync(dataContext);

            var category = await dataContext.WebProductCategories.FirstAsync(c => c.Code == SeedCategoryCode);
            var promotionType = await dataContext.PromotionsType.FirstAsync(x => x.NavisionId == SeedPromotionTypeNavisionId);
            var organicTag = await dataContext.ProductsTags.FirstAsync(x => x.Code == "SEED-TAG-1");

            var milk = new Product
            {
                Code = "SEED-P-MILK",
                Title = "Organic milk 2.5%",
                FamilyCode = "SEED-FAM-1",
                BrandCode = "SEED-BRAND-1",
                ImagePath = "seed-milk.jpg",
                CreatedAt = now,
                UpdatedAt = now,
                ProductTags =
                [
                    new ProductTag { TagId = organicTag.Id, ProductsTag = organicTag },
                ],
            };

            var bread = new Product
            {
                Code = "SEED-P-BREAD",
                Title = "Whole grain bread",
                FamilyCode = "SEED-FAM-1",
                BrandCode = "SEED-BRAND-1",
                ImagePath = "seed-bread.jpg",
                CreatedAt = now,
                UpdatedAt = now,
            };

            await dataContext.Products.AddRangeAsync(milk, bread);

            await dataContext.Promotions.AddRangeAsync(
                new Promotion
                {
                    Id = SeedPromotionId,
                    Title = "Organic milk discount",
                    Description = "Save on organic milk this week",
                    Price = 49.99m,
                    PromoPrice = 39.99m,
                    Image = string.Empty,
                    StoreCode = null,
                    PromotionTypeId = promotionType.NavisionId,
                    PromotionType = promotionType,
                    ProductCode = milk.Code,
                    Product = milk,
                    WebProductCategory = category,
                    StartedAt = now.AddDays(-3),
                    ExpiredAt = now.AddDays(30),
                    InActive = false,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now,
                    IsComplexPromotion = false,
                    ItemUnit = ItemUnit.Items,
                },
                new Promotion
                {
                    Id = SeedPromotionId + 1,
                    Title = "Fresh bread offer",
                    Description = "Daily bakery special",
                    Price = 32.50m,
                    PromoPrice = 24.90m,
                    Image = string.Empty,
                    StoreCode = null,
                    ProductCode = bread.Code,
                    Product = bread,
                    WebProductCategory = category,
                    StartedAt = now.AddDays(-1),
                    ExpiredAt = now.AddDays(14),
                    InActive = false,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now,
                    IsComplexPromotion = false,
                    ItemUnit = ItemUnit.Items,
                },
                new Promotion
                {
                    Id = SeedPromotionId + 2,
                    Title = "Breakfast combo",
                    Description = "Milk and bread bundle for morning flights",
                    Offer = "Buy milk + bread together",
                    Price = 0m,
                    PromoPrice = 0m,
                    Image = string.Empty,
                    StoreCode = null,
                    WebProductCategory = category,
                    StartedAt = now.AddDays(-2),
                    ExpiredAt = now.AddDays(21),
                    InActive = false,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now,
                    IsComplexPromotion = true,
                    ItemUnit = ItemUnit.Items,
                },
                new Promotion
                {
                    Id = SeedPromotionId + 3,
                    Title = "Expired sample (admin only)",
                    Description = "Used to verify admin lists include expired promotions",
                    Price = 19.99m,
                    PromoPrice = 14.99m,
                    Image = string.Empty,
                    StoreCode = null,
                    WebProductCategory = category,
                    StartedAt = now.AddDays(-60),
                    ExpiredAt = now.AddDays(-1),
                    InActive = false,
                    CreatedAt = now.AddDays(-60),
                    UpdatedAt = now,
                    IsComplexPromotion = false,
                    ItemUnit = ItemUnit.Pack,
                },
                new Promotion
                {
                    Id = SeedPromotionId + 4,
                    Title = "Inactive sample (admin only)",
                    Description = "Hidden from client promotion lists",
                    Price = 25.00m,
                    PromoPrice = 19.00m,
                    Image = string.Empty,
                    StoreCode = null,
                    WebProductCategory = category,
                    StartedAt = now.AddDays(-1),
                    ExpiredAt = now.AddDays(30),
                    InActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsComplexPromotion = false,
                    ItemUnit = ItemUnit.Kilograms,
                });
        }

        private static async Task EnsureSeedProductHierarchyAsync(IntegrationDataContext dataContext)
        {
            if (await dataContext.ProductDivisions.AnyAsync(x => x.Code == "SEED-DIV-1"))
            {
                return;
            }

            await dataContext.ProductDivisions.AddAsync(new ProductDivision
            {
                Code = "SEED-DIV-1",
                Title = "Seed division",
            });
            await dataContext.ProductSegments.AddAsync(new ProductSegment
            {
                Code = "SEED-SEG-1",
                Title = "Seed segment",
                DivisionCode = "SEED-DIV-1",
            });
            await dataContext.ProductCategories.AddAsync(new ProductCategory
            {
                Code = "SEED-PCAT-1",
                Title = "Seed category",
                Image = string.Empty,
                SegmentCode = "SEED-SEG-1",
            });
            await dataContext.ProductManufacturers.AddAsync(new ProductManufacturer
            {
                Code = "SEED-MFR-1",
                Title = "Seed manufacturer",
            });
            await dataContext.ProductFamilies.AddAsync(new ProductFamily
            {
                Code = "SEED-FAM-1",
                Title = "Seed family",
                CategoryCode = "SEED-PCAT-1",
            });
            await dataContext.ProductBrands.AddAsync(new ProductBrand
            {
                Code = "SEED-BRAND-1",
                Title = "Seed brand",
                ManufacturerCode = "SEED-MFR-1",
            });
        }
    }
}
