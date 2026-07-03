using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JetFlight.ApplicationDataAccess.Helpers;

public static class SeedHelper
{
    public static async Task Seed(this ApplicationDataContext dataContext, IConfiguration configuration)
    {
        await SeedCities(dataContext);
        await SeedAirportHubs(dataContext);
        await SeedRootPages(dataContext);
        await SeedHeaderFooter(dataContext);
        await SeedPostsTags(dataContext);
        await SeedPermissions(dataContext);
        await SeedSuperAdmin(dataContext, configuration);
        await SeedTopics(dataContext);
        await SeedSiteSettings(dataContext);
        await dataContext.SaveChangesAsync();
    }

    private static async Task SeedPostsTags(ApplicationDataContext dataContext)
    {
        var labels = SeedRandomizer.Labels("Tag", 12);
        var postTags = labels
            .Select((label, index) => new PostTag
            {
                Id = index + 1,
                Name = label,
                Title = label,
                Icon = "tag",
            })
            .ToList();

        foreach (var postTag in postTags)
        {
            if (!await dataContext.PostsTags.AnyAsync(x => x.Id == postTag.Id))
            {
                await dataContext.PostsTags.AddAsync(postTag);
            }
        }
    }

    private static async Task SeedHeaderFooter(ApplicationDataContext dataContext)
    {
        var pages = new List<Page>
        {
            new()
            {
                Id = PageConstants.BirdJetHeaderAndFooterPageId,
                Name = SeedRandomizer.Label("Layout"),
                Title = SeedRandomizer.Label("Layout"),
                RootPage = RootPage.KeyValues,
                BranchId = (byte)Branches.BirdJet,
                IsActive = true,
            },
            new()
            {
                Id = PageConstants.CatJetHeaderAndFooterPageId,
                Name = SeedRandomizer.Label("Layout"),
                Title = SeedRandomizer.Label("Layout"),
                RootPage = RootPage.KeyValues,
                BranchId = (byte)Branches.CatJet,
                IsActive = true,
            },
        };

        foreach (var page in pages)
        {
            if (await dataContext.Page.AnyAsync(x => x.Id == page.Id))
            {
                continue;
            }

            await dataContext.Page.AddAsync(page);
            await dataContext.Sections.AddRangeAsync(
                new Section
                {
                    PageId = page.Id,
                    Name = SeedRandomizer.Label("Header"),
                    Title = "Header",
                    Position = 1,
                    IsActive = true,
                    IsHtml = false,
                    CreatedAt = DateTime.UtcNow.SetKindUtc(),
                    SectionFields =
                    [
                        new SectionField { Key = "header.logo.link", Title = "Logo", Type = "image", Position = 0 },
                        new SectionField { Key = "header.link1.name", Title = "Link 1", Position = 1 },
                        new SectionField { Key = "header.link1.link", Title = "URL", Position = 2, RelatedTitle = "Link 1" },
                    ],
                },
                new Section
                {
                    PageId = page.Id,
                    Name = SeedRandomizer.Label("Footer"),
                    Title = "Footer",
                    Position = 2,
                    IsActive = true,
                    IsHtml = false,
                    CreatedAt = DateTime.UtcNow.SetKindUtc(),
                    SectionFields =
                    [
                        new SectionField { Key = "footer.block1.status.isActive", Value = "true", Type = "boolean", Position = 1 },
                        new SectionField { Key = "footer.block1.block.name", Title = "Column 1", Position = 2 },
                    ],
                });
        }
    }

    private static async Task SeedCities(ApplicationDataContext dataContext)
    {
        for (var id = 1; id <= 7; id++)
        {
            if (await dataContext.Cities.AnyAsync(x => x.Id == id))
            {
                continue;
            }

            await dataContext.Cities.AddAsync(new City
            {
                Id = id,
                Name = SeedRandomizer.Token("CITY"),
            });
        }
    }

    private static async Task SeedAirportHubs(ApplicationDataContext dataContext)
    {
        var cities = await dataContext.Cities.AsNoTracking().ToListAsync();
        var hubId = 1;

        foreach (var city in cities)
        {
            for (var slot = 0; slot < 2; slot++)
            {
                if (await dataContext.Stores.AnyAsync(x => x.Id == hubId))
                {
                    hubId++;
                    continue;
                }

                await dataContext.Stores.AddAsync(new Store
                {
                    Id = hubId,
                    Number = SeedRandomizer.Token("HUB"),
                    MapLink = new Uri("https://maps.example.test/hub"),
                    Title = SeedRandomizer.Label("Airport hub"),
                    Address = SeedRandomizer.Token("TERMINAL"),
                    Address2 = SeedRandomizer.Token("GATE"),
                    CityId = city.Id,
                    Region = SeedRandomizer.Token("REGION"),
                    isActive = true,
                    Latitude = "0",
                    Longitude = "0",
                    BranchId = city.Id % 2 == 0 ? (byte)Branches.CatJet : (byte)Branches.BirdJet,
                });

                hubId++;
            }
        }

        var dayKeys = Enum.GetValues<Day>().ToList();
        await dataContext.Stores
            .Include(x => x.WorkingHours)
            .ForEachAsync(store =>
            {
                var workingHoursToAdd = dayKeys
                    .Where(day => store.WorkingHours.All(x => x.Day != day))
                    .Select(day => new WorkingHours
                    {
                        Day = day,
                        IsActive = true,
                        OpeningTime = new TimeSpan(6, 0, 0),
                        ClosingTime = new TimeSpan(22, 0, 0),
                    });

                foreach (var hours in workingHoursToAdd)
                {
                    store.WorkingHours.Add(hours);
                    dataContext.Entry(hours).State = EntityState.Added;
                }
            });
    }

    private static async Task SeedRootPages(ApplicationDataContext dataContext)
    {
        var birdJetPages = new (int Id, int Order, RootPage RootPage, string Link)[]
        {
            (1, 1, RootPage.Home, "/"),
            (2, 2, RootPage.Bonuses, "/discount"),
            (3, 3, RootPage.Vouchers, "/vouchers"),
            (4, 4, RootPage.Offers, "/holiday-offers"),
            (6, 7, RootPage.Charity, "/charity"),
            (7, 8, RootPage.Contacts, "/contacts"),
            (16, 9, RootPage.Policies, "/policy"),
            (37, 5, RootPage.Accumulation, "/cardplusone"),
            (40, 10, RootPage.Cookies, "/cookies"),
        };

        var catJetPages = birdJetPages
            .Select(page => (page.Id + 298, page.Order, page.RootPage, page.Link))
            .ToArray();

        await SeedPagesForBranch(dataContext, birdJetPages, Branches.BirdJet);
        await SeedPagesForBranch(dataContext, catJetPages, Branches.CatJet);
    }

    private static async Task SeedPagesForBranch(
        ApplicationDataContext dataContext,
        IEnumerable<(int Id, int Order, RootPage RootPage, string Link)> pages,
        Branches branch)
    {
        foreach (var page in pages)
        {
            if (await dataContext.Page.AnyAsync(x => x.Id == page.Id))
            {
                continue;
            }

            var label = SeedRandomizer.Label(page.RootPage.ToString());
            await dataContext.Page.AddAsync(new Page
            {
                Id = page.Id,
                Order = page.Order,
                Name = label,
                Title = label,
                Link = page.Link,
                RootPage = page.RootPage,
                BranchId = (byte)branch,
                IsActive = true,
            });
        }
    }

    private static async Task SeedPermissions(ApplicationDataContext dataContext)
    {
        var permissions = Enum.GetValues<Permission>()
            .SelectMany(permission => Enum.GetValues<PermissionLevel>().Select(level => new RolesPermission
            {
                Title = permission.GetEnumMemberValue(),
                EntityType = permission.ToString(),
                Crud = (byte)level,
            }));

        foreach (var permission in permissions)
        {
            if (!await dataContext.RolesPermissions.AnyAsync(x =>
                    x.EntityType == permission.EntityType && x.Crud == permission.Crud))
            {
                await dataContext.RolesPermissions.AddAsync(permission);
            }
        }
    }

    private static async Task SeedSuperAdmin(ApplicationDataContext dataContext, IConfiguration configuration)
    {
        var settings = configuration.GetSection("SuperAdmin").Get<SuperAdminSettings>();
        if (settings == null || await dataContext.Admins.AnyAsync(x => x.Email == settings.Email))
        {
            return;
        }

        var password = CryptographyHelper.HashPassword(settings.DefaultPasword, out var salt);
        await dataContext.Admins.AddAsync(new Admin
        {
            Email = settings.Email,
            FirstName = SeedRandomizer.Label("Admin"),
            LastName = SeedRandomizer.Token("USER"),
            Title = "SuperAdmin",
            PhoneNumber = string.Empty,
            IsSuperadmin = true,
            Password = password,
            Blocked = false,
            Salt = salt == null ? null : Convert.ToBase64String(salt),
        });
    }

    private static async Task SeedTopics(ApplicationDataContext dataContext)
    {
        for (var id = 1; id <= 8; id++)
        {
            if (await dataContext.Topics.AnyAsync(x => x.Id == id))
            {
                continue;
            }

            var label = SeedRandomizer.Label("Topic");
            await dataContext.Topics.AddAsync(new Topic
            {
                Id = id,
                Name = label,
                Title = label,
            });
        }
    }

    private static async Task SeedSiteSettings(ApplicationDataContext dataContext)
    {
        foreach (var branch in Enum.GetValues<Branches>())
        {
            foreach (var key in Enum.GetValues<SiteSettingsKeys>())
            {
                if (await dataContext.SiteSettings.AnyAsync(x => x.BranchId == (byte)branch && x.Key == key))
                {
                    continue;
                }

                await dataContext.SiteSettings.AddAsync(new SiteSettings
                {
                    BranchId = (byte)branch,
                    Key = key,
                    Value = SeedRandomizer.Token("SETTING"),
                });
            }
        }
    }
}
