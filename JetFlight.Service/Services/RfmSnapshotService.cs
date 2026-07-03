using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JetFlight.Service.Services
{
    public interface IRfmSnapshotService
    {
        Task GenerateDailySnapshotAsync(DateOnly snapshotDate, CancellationToken cancellationToken);
    }

    public class RfmSnapshotService : IRfmSnapshotService
    {
        private readonly IntegrationDataContext _integrationContext;
        private readonly ApplicationDataContext _applicationContext;
        private readonly ILogger<RfmSnapshotService> _logger;

        private sealed class CustomerLocation
        {
            public byte? BranchId { get; set; }
            public int? StoreId { get; set; }
            public int? CityId { get; set; }
        }

        public RfmSnapshotService(
            IntegrationDataContext integrationContext,
            ApplicationDataContext applicationContext,
            ILogger<RfmSnapshotService> logger)
        {
            _integrationContext = integrationContext;
            _applicationContext = applicationContext;
            _logger = logger;
        }

        public async Task GenerateDailySnapshotAsync(DateOnly snapshotDate, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Starting RFM snapshot generation for {SnapshotDate}", snapshotDate);

            var snapshotEndLocal = snapshotDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
            var snapshotEndUtc = snapshotEndLocal.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);

            var allRfms = await _applicationContext.RFMs
                .AsNoTracking()
                .Include(r => r.Period)
                .Include(r => r.Count)
                .Include(r => r.Amount)
                .ToListAsync(cancellationToken);

            if (allRfms.Count == 0)
            {
                _logger.LogWarning("No RFM definitions were found. Skipping snapshot for {SnapshotDate}", snapshotDate);

                await _integrationContext.RfmCustomerSnapshots
                    .Where(x => x.SnapshotDate == snapshotDate)
                    .ExecuteDeleteAsync(cancellationToken);

                return;
            }

            var receiptTotals = await _integrationContext.Receipts
                .AsNoTracking()
                .Where(r => r.CustomerCard.CustomerId.HasValue && r.CreatedAt < snapshotEndUtc)
                .Select(r => new
                {
                    CustomerId = r.CustomerCard.CustomerId!.Value,
                    r.CreatedAt,
                    Total = r.ReceiptProducts.Sum(p => (decimal?)(p.Price * p.Quantity - p.Discount)) ?? 0m
                })
                .ToListAsync(cancellationToken);

            var customerStats = receiptTotals
                .GroupBy(r => r.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    LastPurchase = g.Max(x => x.CreatedAt),
                    Frequency = g.Count(),
                    Monetary = g.Sum(x => x.Total)
                })
                .ToList();

            await _integrationContext.RfmCustomerSnapshots
                .Where(x => x.SnapshotDate == snapshotDate)
                .ExecuteDeleteAsync(cancellationToken);

            if (customerStats.Count == 0)
            {
                _logger.LogInformation("No receipts were found before {SnapshotDate}. Snapshot table cleared.", snapshotDate);
                return;
            }

            var customerIds = customerStats.Select(x => x.CustomerId).ToList();

            var settings = await _integrationContext.CustomerSettings
                .AsNoTracking()
                .Where(s => customerIds.Contains(s.CustomerId))
                .ToListAsync(cancellationToken);

            var customerLocations = settings
                .GroupBy(s => s.CustomerId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var first = g.First();
                        var storeId = g.Select(x => x.ActiveStoreId).FirstOrDefault(x => x.HasValue);

                        return new CustomerLocation
                        {
                            BranchId = first.BranchId,
                            StoreId = storeId
                        };
                    });

            var storeIds = customerLocations.Values
                .Where(x => x.StoreId.HasValue)
                .Select(x => x.StoreId!.Value)
                .Distinct()
                .ToList();

            if (storeIds.Count > 0)
            {
                var storeCities = await _applicationContext.Stores
                    .AsNoTracking()
                    .Where(s => storeIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.CityId })
                    .ToListAsync(cancellationToken);

                var storeCityMap = storeCities.ToDictionary(x => x.Id, x => x.CityId);

                foreach (var location in customerLocations.Values)
                {
                    if (location.StoreId.HasValue &&
                        storeCityMap.TryGetValue(location.StoreId.Value, out var cityId))
                    {
                        location.CityId = cityId;
                    }
                }
            }

            var snapshots = new List<RfmCustomerSnapshot>(customerStats.Count);

            foreach (var stat in customerStats)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var recencyDays = (int)Math.Max(0, (snapshotEndUtc - stat.LastPurchase).TotalDays);
                var monetaryValue = (int)Math.Round(stat.Monetary, MidpointRounding.AwayFromZero);

                var segment = allRfms.FirstOrDefault(r =>
                    r.Period != null && r.Count != null && r.Amount != null &&
                    recencyDays >= r.Period.From && recencyDays <= r.Period.To &&
                    stat.Frequency >= r.Count.From && stat.Frequency <= r.Count.To &&
                    monetaryValue >= r.Amount.From && monetaryValue <= r.Amount.To);

                if (segment == null)
                {
                    continue;
                }

                customerLocations.TryGetValue(stat.CustomerId, out var location);

                snapshots.Add(new RfmCustomerSnapshot
                {
                    CustomerId = stat.CustomerId,
                    RfmId = segment.Id,
                    SnapshotDate = snapshotDate,
                    BranchId = location?.BranchId,
                    StoreId = location?.StoreId,
                    CityId = location?.CityId
                });
            }

            if (snapshots.Count == 0)
            {
                _logger.LogInformation("No snapshots were produced for {SnapshotDate}.", snapshotDate);
                return;
            }

            await _integrationContext.RfmCustomerSnapshots.AddRangeAsync(snapshots, cancellationToken);
            await _integrationContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Persisted {SnapshotCount} RFM snapshots for {SnapshotDate}.", snapshots.Count, snapshotDate);
        }
    }
}
