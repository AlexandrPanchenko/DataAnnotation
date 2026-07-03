using System.Linq.Expressions;
using JetFlight.ApplicationDataAccess;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.Service.Services;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.UserContext;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;

namespace JetFlight.WebApiTests.Services.FlightLoyalty;

internal sealed class FlightLoyaltyServiceTestContext : IDisposable
{
    private readonly SqliteConnection _integrationConnection;

    public FlightLoyaltyServiceTestContext(IReadOnlyList<Store>? stores = null)
    {
        stores ??= FlightLoyaltyTestDataBuilder.DefaultStores;

        _integrationConnection = new SqliteConnection("Data Source=:memory:");
        _integrationConnection.Open();

        UserContext = new UserContext();
        IntegrationContext = CreateIntegrationContext();
        IntegrationContext.Database.EnsureCreated();

        JobSchedulerMock = new Mock<IJobSchedulerService>();
        MemoryCache = new MemoryCache(new MemoryCacheOptions());
        Stores = stores.ToList();

        var storeRepoMock = new Mock<IStoreRepository>();
        storeRepoMock.Setup(r => r.GetAllStores()).Returns(CreateMockQueryable(Stores));
        storeRepoMock.Setup(r => r.GetAll()).Returns(CreateMockQueryable(Stores));
        storeRepoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Store, bool>>>()))
            .Returns((Expression<Func<Store, bool>> predicate) =>
                CreateMockQueryable(Stores.Where(predicate.Compile()).ToList()));

        var unitOfWorkMock = new Mock<IDataUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Stores).Returns(storeRepoMock.Object);

        Service = new FlightLoyaltyService(
            IntegrationContext,
            UserContext,
            unitOfWorkMock.Object,
            MemoryCache,
            Mock.Of<IGlobalSearchService>(),
            Mock.Of<IMediaService>(),
            JobSchedulerMock.Object,
            new ApplicationDataContext(),
            Mock.Of<IFirebaseService>(),
            Mock.Of<INotificationService>(),
            Mock.Of<IHtmlGenerationService>(),
            Options.Create(new SmsSettings()));
    }

    public UserContext UserContext { get; }
    public IntegrationDataContext IntegrationContext { get; }
    public FlightLoyaltyService Service { get; }
    public Mock<IJobSchedulerService> JobSchedulerMock { get; }
    public MemoryCache MemoryCache { get; }
    public List<Store> Stores { get; }

    public Task SeedAsync(Action<IntegrationDataContext>? extraSeed = null)
    {
        if (extraSeed == null)
        {
            return SeedAsync((Func<IntegrationDataContext, Task>?)null);
        }

        return SeedAsync(c =>
        {
            extraSeed(c);
            return Task.CompletedTask;
        });
    }

    public async Task SeedAsync(Func<IntegrationDataContext, Task>? extraSeed)
    {
        await FlightLoyaltyTestDataBuilder.SeedCatalogAsync(IntegrationContext);
        await FlightLoyaltyTestDataBuilder.SeedDisplayRulesAsync(IntegrationContext);
        await IntegrationContext.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);

        if (extraSeed != null)
        {
            await extraSeed(IntegrationContext);
        }

        await IntegrationContext.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);
    }

    private TestIntegrationDataContext CreateIntegrationContext()
    {
        var busMock = new Mock<IBus>();
        return new TestIntegrationDataContext(_integrationConnection, busMock.Object, UserContext);
    }

    private static IQueryable<T> CreateMockQueryable<T>(IReadOnlyList<T> source)
        where T : class
    {
        return source.AsQueryable().BuildMock();
    }

    public void Dispose()
    {
        IntegrationContext.Dispose();
        MemoryCache.Dispose();
        _integrationConnection.Dispose();
    }
}
