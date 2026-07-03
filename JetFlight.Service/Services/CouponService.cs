using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.Product;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using JetFlight.Shared.Models.LogHistory;
using Newtonsoft.Json;
using System.Threading;
using MassTransit;

namespace JetFlight.Service.Services
{
    public interface ICouponService
    {
        Task ArchiveAsync(int id);
        Task<AssignedCustomerCouponDTO> GetAvailableCouponByCustomerAsync(int customerCouponId);
        Task<PagedListDTO<AssignedCustomerCouponDTO>> GetAvailableCouponsByCustomerAsync(PagingDTO pagingDTO, int? storeId = null);
        Task<AdminCouponDTO> GetByAdminAsync(int id);

        Task<PagedListDTO<CustomerCouponForAdminDTO>> GetCustomerCouponsByAdminAsync(int customerId, PagingDTO pagingDTO, RangeDTO<DateTime>? startDate = null, RangeDTO<DateTime>? expirationDate = null, CustomerCouponStatus? status = null);

        Task<CustomerCouponForAdminDTO> GetCustomerCouponByAdminAsync(int customerCouponId);


        Task<PagedListDTO<AdminCouponDTO>> GetCouponsByAdminAsync(
            PagingDTO pagingDTO,
            AdminCouponFilterDTO filter
            );
        Task PublishAsync(int id);
        Task SetActiveCustomerCouponAsync(int customerCouponId, bool activeFlag);
        Task SetAllCustomerCouponsAsActiveAsync(bool activeFlag, int? storeId = null);
        Task UpdateAsync(UpdateCouponDTO model);
        Task<AdminCouponDTO> CreateAsync(CreateCouponDTO model);
        Task<int> AssignPersonalCouponToCustomerAsync(int couponId, int customerId);

        Task<CustomerCouponCountDTO> CountForCustomerAsync(int? storeId = null);

        Task<List<AssignedCustomerCouponDTO>> GetAllCustomerCouponsByCustomerId(int customerId, byte branchId);

        Task UseCouponsAsync(int customerId, HashSet<int> customerCouponIds);

        Task DeleteAsync(int id);
    }

    public class CouponService : ICouponService
    {
        private readonly IUserContext _userContext;
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly ITargetService _targetService;
        private readonly IDataUnitOfWork _appUnitOfWork;
        private readonly IJobSchedulerService _jobSchedulerService;
        private readonly ILogHistoryService _logHistoryService;
        private readonly IBus _bus;

        public CouponService(
            IUserContext userContext,
            IntegrationDataContext integrationDataContext,
            ITargetService targetService,
            IDataUnitOfWork appUnitOfWork,
            IJobSchedulerService jobSchedulerService,
            ILogHistoryService logHistoryService,
            IBus bus)
        {
            _userContext = userContext;
            _integrationDataContext = integrationDataContext;
            _targetService = targetService;
            _appUnitOfWork = appUnitOfWork;
            _jobSchedulerService = jobSchedulerService;
            _logHistoryService = logHistoryService;
            _bus = bus;
        }

        public async Task<AssignedCustomerCouponDTO> GetAvailableCouponByCustomerAsync(int customerCouponId)
        {
            var customerId = _userContext.CustomerId;
            var currentDate = DateTime.UtcNow.SetKindUtc();

            var query = await GetBaseCustomerQueryAsync(_userContext.CustomerId!.Value, (byte)_userContext.BranchId!.Value);
            var customerCoupon = await query
                .FirstOrDefaultAsync(x => x.Id == customerCouponId);

            if (customerCoupon == null)
            {
                throw new ArgumentException("Активного ваучера з таким id немає");
            }

            return await ToAssignedCustomerCouponDTOAsync(customerCoupon);
        }

        public async Task<PagedListDTO<AssignedCustomerCouponDTO>> GetAvailableCouponsByCustomerAsync(PagingDTO pagingDTO, int? storeId = null)
        {
            var customerId = _userContext.CustomerId;
            var currentDate = DateTime.UtcNow.SetKindUtc();
            var query = await GetBaseCustomerQueryAsync(_userContext.CustomerId!.Value, (byte) _userContext.BranchId!.Value);

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                query = query.Where(x => x.Coupon.StoreCodes.Any(x => x.StoreCode == storeCode));
            }

            return await query.GetPagedListAsync(pagingDTO, ToAssignedCustomerCouponDTOAsync);
        }

        private async Task<IQueryable<CustomerCoupon>> GetBaseCustomerQueryAsync(int customerId, byte? branchId)
        {
            var query = _integrationDataContext.CustomerCoupons
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        //////.ThenInclude(x => x.Product)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        //.ThenInclude(x => x.ProductActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.BrandActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationFixedPrice)
                        .ThenInclude(x => x.Activators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        //.ThenInclude(x => x.ProductActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.BrandActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponDiscountPercent)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponDiscountAmount)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponProductFixedPrice)
                        //.ThenInclude(x => x.Product)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.StoreCodes)
                .Where(GetCustomerCouponPredicate(customerId));

            if (branchId.HasValue)
            {
                var storeCodes = await _appUnitOfWork.Stores.Find(x => x.BranchId == branchId)
                .Select(x => x.Number).ToListAsync();
                query = query.Where(x => x.Coupon.StoreCodes.Any(x => storeCodes.Contains(x.StoreCode)));
            }

            return query;
        }

        private static Expression<Func<CustomerCoupon, bool>> GetCustomerCouponPredicate(int customerId)
        {
            var currentDate = DateTime.UtcNow.SetKindUtc();
            return x => x.Coupon.UseTimes > x.UsedTimes
                    && x.Coupon.StartDate <= currentDate
                    && x.Coupon.ExpirationDate >= currentDate
                    && x.Coupon.Status == CouponStatus.Active
                    && x.CustomerId == customerId;
        }

        private IQueryable<Coupon> GetAdminQuery()
            => _integrationDataContext.Coupons
                .Include(x => x.Targets)
                .Include(x => x.CouponCombinationPriceDiscount)
                    .ThenInclude(x => x.ProductActivators)
                .Include(x => x.CouponCombinationPriceDiscount)
                    .ThenInclude(x => x.Product)
                .Include(x => x.CouponCombinationPriceDiscount)
                    .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.CouponCombinationPriceDiscount)
                    .ThenInclude(x => x.BrandActivators)
                .Include(x => x.CouponCombinationPriceDiscount)
                    .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.CouponCombinationPriceDiscount)
                    .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.CouponCombinationPriceDiscount)
                .Include(x => x.CouponCombinationFixedPrice)
                    .ThenInclude(x => x.Activators)
                .Include(x => x.CouponAdditionalBonus)
                .Include(x => x.CouponBonusMultiplier)
                .Include(x => x.CouponDiscountPercent)
                .Include(x => x.CouponDiscountAmount)
                .Include(x => x.CouponProductFixedPrice)
                    .ThenInclude(x => x.Product)
                .Include(x => x.StoreCodes)
            .Where(x => !x.IsCardCoupon);

        public async Task PublishAsync(int id)
        {
            var coupon = await _integrationDataContext.Coupons
                .Include(x => x.Targets)
                .FirstOrDefaultAsync(x => x.Id == id
                    && !x.IsCardCoupon);
            if (coupon == null)
            {
                throw new ArgumentException("Ваучер не знайдений");
            }

            if (coupon.Status != CouponStatus.Inactive)
            {
                throw new ArgumentException("Тільки неактивний ваучер можно запаблішити");
            }

            if (coupon.Class == CouponClass.Common)
            {
                var connection = _integrationDataContext.Database.GetDbConnection();
                await connection.OpenAsync();
                var createTempTableCommand = connection.CreateCommand();
                createTempTableCommand.CommandText = @"
DROP TABLE IF EXISTS #CustomersForCoupons;
CREATE TABLE #CustomersForCoupons
(
Id integer NOT NULL,
);";
                await createTempTableCommand.ExecuteNonQueryAsync();

                foreach (var t in coupon.Targets)
                {
                    var target = await _targetService.GetTargetEntityAsync(t.TargetId);
                    await _targetService.PopulateTargetCustomersIntoTempTableAsync(target, connection);

                    var insertTempTableQuery = connection.CreateCommand();
                    insertTempTableQuery.CommandText = @"
INSERT INTO #CustomersForCoupons
SELECT DISTINCT c.Id
FROM #customers c
LEFT JOIN #CustomersForCoupons ct ON ct.Id = c.Id
WHERE ct.Id IS NULL;
";
                    await insertTempTableQuery.ExecuteNonQueryAsync();
                }

                var command = connection.CreateCommand();
                command.CommandText = @"
BEGIN TRANSACTION;

UPDATE Coupons
SET Status = 1, UpdatedAt = GETUTCDATE()
WHERE Id = @CouponId;

INSERT INTO CustomerCoupons (CustomerId, CouponId, Activated, UsedTimes, AssignedAt)
SELECT TOP (@Emission) 
    c.Id, @CouponId, 0, 0, GETUTCDATE()
FROM #CustomersForCoupons c
ORDER BY 
    CASE 
        WHEN @EmissionBy = 0 THEN CHECKSUM(NEWID())
        WHEN @EmissionBy = 1 THEN (
            SELECT ISNULL(SUM(t.Amount), 0) FROM (
                SELECT SUM(rp.Quantity * rp.Price - rp.Discount) AS Amount 
                FROM Receipts r
                JOIN ReceiptProducts rp ON rp.ReceiptId = r.Id
                JOIN CustomerCards cc on cc.Code = r.CardCode
                WHERE cc.CustomerId = c.Id
                GROUP BY r.Id
            ) t
        )
        WHEN @EmissionBy = 2 THEN (
            SELECT ISNULL(SUM(t.Amount), 0) FROM (
                SELECT SUM(rp.Quantity * rp.Price - rp.Discount) AS Amount 
                FROM Receipts r
                JOIN ReceiptProducts rp ON rp.ReceiptId = r.Id
                JOIN CustomerCards cc on cc.Code = r.CardCode
                WHERE cc.CustomerId = c.Id
                GROUP BY r.Id
            ) t
        ) * -1
        WHEN @EmissionBy = 3 THEN (
            SELECT COUNT(1) FROM Receipts r JOIN CustomerCards cc on cc.Code = r.CardCode WHERE cc.CustomerId = c.Id
        )
        WHEN @EmissionBy = 4 THEN (
            SELECT COUNT(1) FROM Receipts r JOIN CustomerCards cc on cc.Code = r.CardCode WHERE cc.CustomerId = c.Id    
        ) * -1
    END;

COMMIT TRANSACTION;

";

                var couponIdParam = command.CreateParameter();
                couponIdParam.ParameterName = "@CouponId";
                couponIdParam.Value = coupon.Id;
                command.Parameters.Add(couponIdParam);

                var emissionParam = command.CreateParameter();
                emissionParam.ParameterName = "@Emission";
                emissionParam.Value = coupon.Emission;
                command.Parameters.Add(emissionParam);

                var emissionByParam = command.CreateParameter();
                emissionByParam.ParameterName = "@EmissionBy";
                emissionByParam.Value = coupon.EmissionBy;
                command.Parameters.Add(emissionByParam);

                await command.ExecuteNonQueryAsync();
                await _logHistoryService.AddRangeAsync(LogHistoryHelper.GetPublishCouponLog(coupon.Id, _userContext.AdminId));
            }
            else
            {
                coupon.Status = CouponStatus.Active;
                coupon.UpdatedAt = DateTime.UtcNow.SetKindUtc();
                await _integrationDataContext.SaveChangesAsync();
            }

            if (coupon.ExpirationDate <= DateTime.UtcNow.AddHours(JobConstants.LoyaltyExpirationJobIntervalHours))
            {
                await _jobSchedulerService.SetCouponExpirationJobAsync(coupon.Id, coupon.ExpirationDate, true);
            }

            if (coupon.Class == CouponClass.Common && coupon.StartDate < DateTime.UtcNow.AddMinutes(30))
            {
                await _jobSchedulerService.SetCouponActiveNotificationJobAsync(coupon.Id, coupon.StartDate, true);
            }
        }

        public async Task ArchiveAsync(int id)
        {
            var coupon = await _integrationDataContext.Coupons
                .Include(x => x.Questionaries)
                .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsCardCoupon);
            
            if (coupon == null)
            {
                throw new ArgumentException("Ваучер не знайдений");
            }

            if (coupon.Status == CouponStatus.Archived)
            {
                throw new ArgumentException("Ваучер вже заархівований");
            }

            coupon.Status = CouponStatus.Archived;
            coupon.UpdatedAt = DateTime.UtcNow.SetKindUtc();

            var questionariesToArchive = coupon.Questionaries.Where(x => x.Status == Shared.Models.Questionary.QuestionaryStatus.Activated).ToList();

            foreach (var questionary in questionariesToArchive)
            {
                questionary.Status = Shared.Models.Questionary.QuestionaryStatus.Archived;
                questionary.UpdatedAt = DateTime.UtcNow;
            };

            await _integrationDataContext.SaveChangesAsync();

            await _jobSchedulerService.RemoveCouponExpirationJobAsync(coupon.Id);

            foreach (var questionary in questionariesToArchive)
            {
                await _jobSchedulerService.RemoveQuestionaryExpirationJobAsync(questionary.Id);
            };
        }

        public async Task<AdminCouponDTO> CreateAsync(CreateCouponDTO model)
        {
            model.TargetIds ??= new List<int>();
            if ((model.Class == CouponClass.Common) != model.TargetIds.Any())
            {
                throw new ArgumentException("Помилка в таргеті або типі");
            }

            if (model.Class == CouponClass.Common && !model.EmissionBy.HasValue)
            {
                throw new ArgumentException("Оберіть розподілення по еміссії");
            }

            if (model.Class == CouponClass.Personal && model.EmissionBy.HasValue)
            {
                throw new ArgumentException("Розподілення по еміссії доступнє тільки для загальних купонів");
            }

            var storeCodes = await GetStoreCodesThrowIfNotExistAsync(model.StoreIds);

            var coupon = new Coupon
            {
                Class = model.Class,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                Description = model.Description,
                UseTimes = model.UseTimes,
                ExpirationDate = model.ExpirationDate,
                EmissionBy = model.EmissionBy,
                Name = model.Name,
                PrivateName = model.PrivateName,
                PrivateDescription = model.PrivateDescription,
                StartDate = model.StartDate,
                StoreCodes = storeCodes.Select(x => new CouponToStore { StoreCode = x }).ToList(),
                Status = CouponStatus.Inactive,
                Emission = model.Emission,
                Targets = model.TargetIds.Select(x => new CouponToTarget
                {
                    TargetId = x
                }).ToList(),
                Image = model.Image,
            };

            await SetDetailsAsync(model.Details, coupon);

            await _integrationDataContext.Coupons.AddAsync(coupon);
            await _integrationDataContext.SaveChangesAsync(ignoreLogs: true, cancellationToken: CancellationToken.None);

            LogMessage logMessage = new LogMessage
            {
                AdminId = _userContext.AdminId,
                EntityType = "Coupons",
                UpdatedFrom = null,
                UpdatedTo = JsonConvert.SerializeObject(new Coupon
                {
                    Id = coupon.Id,
                    Name = coupon.Name,
                    PrivateName = coupon.PrivateName,
                    Description = coupon.Description,
                    PrivateDescription = coupon.PrivateDescription,
                    Image = coupon.Image,
                    StartDate = coupon.StartDate,
                    ExpirationDate = coupon.ExpirationDate,
                    EmissionBy = coupon.EmissionBy,
                    CreatedAt = coupon.CreatedAt,
                    IsCardCoupon = coupon.IsCardCoupon,
                    UseTimes = coupon.UseTimes,
                    Emission = coupon.Emission,
                    Class = coupon.Class,
                    Status = coupon.Status,
                    Type = coupon.Type,
                }, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                }),
                EntityId = coupon.Id,
                Action = ActionConstant.Inserted,
                Date = DateTime.UtcNow,
            };

            await _logHistoryService.AddAsync(logMessage);

            return await ToAdminCouponDTOAsync(coupon);
        }

        private async Task SetDetailsAsync(CouponDetailsDTO details, Coupon coupon)
        {
            switch (details)
            {
                case CouponAdditionalBonusDTO x:
                    {
                        coupon.Type = CouponType.AdditionalBonus;
                        coupon.CouponAdditionalBonus ??= new CouponAdditionalBonus();
                        coupon.CouponAdditionalBonus.Bonus = x.Bonus;

                        break;
                    }
                case CouponBonusMultiplierDTO x:
                    {
                        coupon.Type = CouponType.BonusMultiplier;
                        coupon.CouponBonusMultiplier ??= new CouponBonusMultiplier();
                        coupon.CouponBonusMultiplier.Multiplier = x.Multiplier;

                        // Map activators
                        coupon.CouponBonusMultiplier.ProductActivators ??= new List<CouponMultiplierProductActivator>();
                        coupon.CouponBonusMultiplier.ManufacturerActivators ??= new List<CouponMultiplierManufacturerActivator>();
                        coupon.CouponBonusMultiplier.SupplierActivators ??= new List<CouponMultiplierSupplierActivator>();
                        coupon.CouponBonusMultiplier.BrandActivators ??= new List<CouponMultiplierBrandActivator>();
                        coupon.CouponBonusMultiplier.CategoryActivators ??= new List<CouponMultiplierCategoryActivator>();

                        coupon.CouponBonusMultiplier.ProductActivators.Clear();
                        coupon.CouponBonusMultiplier.ManufacturerActivators.Clear();
                        coupon.CouponBonusMultiplier.SupplierActivators.Clear();
                        coupon.CouponBonusMultiplier.BrandActivators.Clear();
                        coupon.CouponBonusMultiplier.CategoryActivators.Clear();

                        if (x.Activators != null)
                        {
                            foreach (var activator in x.Activators)
                            {
                                switch (activator)
                                {
                                    case CouponProductActivatorDTO productActivator:
                                        coupon.CouponBonusMultiplier.ProductActivators.Add(new CouponMultiplierProductActivator
                                        {
                                            ProductCode = productActivator.ProductCode,
                                            Quantity = productActivator.Quantity,
                                            CouponBonusMultiplier = coupon.CouponBonusMultiplier
                                        });
                                        break;
                                    case CouponManufacturerActivatorDTO manufacturerActivator:
                                        coupon.CouponBonusMultiplier.ManufacturerActivators.Add(new CouponMultiplierManufacturerActivator
                                        {
                                            ManufacturerCode = manufacturerActivator.ManufacturerCode,
                                            CouponBonusMultiplier = coupon.CouponBonusMultiplier
                                        });
                                        break;
                                    case CouponSupplierActivatorDTO supplierActivator:
                                        coupon.CouponBonusMultiplier.SupplierActivators.Add(new CouponMultiplierSupplierActivator
                                        {
                                            SupplierCode = supplierActivator.SupplierCode,
                                            CouponBonusMultiplier = coupon.CouponBonusMultiplier
                                        });
                                        break;
                                    case CouponBrandActivatorDTO brandActivator:
                                        coupon.CouponBonusMultiplier.BrandActivators.Add(new CouponMultiplierBrandActivator
                                        {
                                            BrandCode = brandActivator.BrandCode,
                                            CouponBonusMultiplier = coupon.CouponBonusMultiplier
                                        });
                                        break;
                                    case CouponCategoryActivatorDTO categoryActivator:
                                        coupon.CouponBonusMultiplier.CategoryActivators.Add(new CouponMultiplierCategoryActivator
                                        {
                                            CategoryCode = categoryActivator.CategoryCode,
                                            CouponBonusMultiplier = coupon.CouponBonusMultiplier
                                        });
                                        break;
                                }
                            }
                        }

                        break;
                    }
                case CouponCombinationFixedPriceDTO x:
                    {
                        coupon.Type = CouponType.CombinationFixedPrice;
                        coupon.CouponCombinationFixedPrice ??= new CouponCombinationFixedPrice
                        {
                            Activators = new List<CouponCombinationFixedPriceActivator>()
                        };

                        coupon.CouponCombinationFixedPrice.FixedPrice = x.FixedPrice;
                        
                        coupon.CouponCombinationFixedPrice.Activators.Clear();

                        coupon.CouponCombinationFixedPrice.Activators.AddRange(
                            x.Activators
                            .Select(a => new CouponCombinationFixedPriceActivator
                            {
                                ProductCode = a.ProductCode,
                                Quantity = a.Quantity,
                            }));

                        break;
                    }
                case CouponCombinationPriceDiscountDTO x:
                    {
                        coupon.Type = CouponType.CombinationPriceDiscount;
                        coupon.CouponCombinationPriceDiscount ??= new CouponCombinationPriceDiscount
                        {
                            ProductActivators = new List<CouponCombinationProductActivator>(),
                            ManufacturerActivators = new List<CouponCombinationManufacturerActivator>(),
                            SupplierActivators = new List<CouponCombinationSupplierActivator>(),
                            BrandActivators = new List<CouponCombinationBrandActivator>(),
                            CategoryActivators = new List<CouponCombinationCategoryActivator>(),
                        };

                        coupon.CouponCombinationPriceDiscount.AllRequired = x.AllRequired;

                        coupon.CouponCombinationPriceDiscount.ProductActivators.Clear();
                        coupon.CouponCombinationPriceDiscount.ManufacturerActivators.Clear();
                        coupon.CouponCombinationPriceDiscount.SupplierActivators.Clear();
                        coupon.CouponCombinationPriceDiscount.BrandActivators.Clear();
                        coupon.CouponCombinationPriceDiscount.CategoryActivators.Clear();

                        foreach (var activator in x.Activators)
                        {
                            switch (activator)
                            {
                                case CouponProductActivatorDTO productActivator:
                                    {
                                        coupon.CouponCombinationPriceDiscount.ProductActivators.Add(
                                            new CouponCombinationProductActivator
                                            {
                                                ProductCode = productActivator.ProductCode,
                                                Quantity = productActivator.Quantity,
                                            });
                                        break;
                                    }
                                case CouponManufacturerActivatorDTO manufacturerActivator: 
                                    {
                                        coupon.CouponCombinationPriceDiscount.ManufacturerActivators.Add(
                                            new CouponCombinationManufacturerActivator
                                            {
                                                ManufacturerCode = manufacturerActivator.ManufacturerCode,
                                            });
                                        break;
                                    }
                                case CouponSupplierActivatorDTO supplierActivator:
                                    {
                                        coupon.CouponCombinationPriceDiscount.SupplierActivators.Add(
                                            new CouponCombinationSupplierActivator
                                            {
                                                SupplierCode = supplierActivator.SupplierCode,
                                            });
                                        break;
                                    }
                                case CouponBrandActivatorDTO brandActivator:
                                    {
                                        coupon.CouponCombinationPriceDiscount.BrandActivators.Add(
                                            new CouponCombinationBrandActivator
                                            {
                                                BrandCode = brandActivator.BrandCode,
                                            });
                                        break;
                                    }
                                case CouponCategoryActivatorDTO categoryActivator:
                                    {
                                        coupon.CouponCombinationPriceDiscount.CategoryActivators.Add(
                                            new CouponCombinationCategoryActivator
                                            {
                                                CategoryCode = categoryActivator.CategoryCode,
                                            });
                                        break;
                                    }
                            }
                        }

                        var product = await _integrationDataContext.Products
                            .FirstOrDefaultAsync(p => p.Code == x.ProductCode);

                        if (product == null)
                        {
                            throw new ArgumentException($"Продукт з кодом {x.ProductCode} не існує");
                        }

                        var supplier = await _integrationDataContext.ProductsSuppliers
                            .FirstOrDefaultAsync(s => s.Code == x.SupplierCode);

                        if (supplier == null)
                        {
                            throw new ArgumentException($"Постачальник з кодом {x.SupplierCode} не існує");
                        }

                        coupon.CouponCombinationPriceDiscount.ProductCode = x.ProductCode;
                        coupon.CouponCombinationPriceDiscount.Quantity = x.Quantity;
                        coupon.CouponCombinationPriceDiscount.Price = x.Price;
                        coupon.CouponCombinationPriceDiscount.Compensation = x.Compensation;
                        coupon.CouponCombinationPriceDiscount.SupplierCode = x.SupplierCode;

                        break;
                    }
                case CouponDiscountAmountDTO x:
                    {
                        coupon.Type = CouponType.DiscountAmount;
                        coupon.CouponDiscountAmount ??= new CouponDiscountAmount();
                        coupon.CouponDiscountAmount.Amount = x.Amount;
                        break;
                    }
                case CouponDiscountPercentDTO x:
                    {
                        coupon.Type = CouponType.DiscountPercent;
                        coupon.CouponDiscountPercent ??= new CouponDiscountPercent();
                        coupon.CouponDiscountPercent.Percent = x.Percent;
                        break;
                    }
                case CouponProductFixedPriceDTO x:
                    {
                        var product = await _integrationDataContext.Products
                            .FirstOrDefaultAsync(p => p.Code == x.ProductCode);

                        if (product == null)
                        {
                            throw new ArgumentException($"Продукт з кодом {x.ProductCode} не існує");
                        }

                        coupon.Type = CouponType.ProductFixedPrice;
                        coupon.CouponProductFixedPrice ??= new CouponProductFixedPrice();
                        coupon.CouponProductFixedPrice.ProductCode = x.ProductCode;
                        coupon.CouponProductFixedPrice.Price = x.Price;
                        coupon.CouponProductFixedPrice.Quanitity = x.Quanitity;

                        break;
                    }
            }

            if (coupon.Type != CouponType.CombinationPriceDiscount && coupon.CouponCombinationPriceDiscount != null)
            {
                _integrationDataContext.CouponCombinationPriceDiscounts.Remove(coupon.CouponCombinationPriceDiscount);
            }

            if (coupon.Type != CouponType.CombinationFixedPrice && coupon.CouponCombinationFixedPrice != null)
            {
                _integrationDataContext.CouponCombinationPriceFixedPrices.Remove(coupon.CouponCombinationFixedPrice);
            }

            if (coupon.Type != CouponType.AdditionalBonus && coupon.CouponAdditionalBonus != null)
            {
                _integrationDataContext.CouponAdditionalBonuses.Remove(coupon.CouponAdditionalBonus);
            }

            if (coupon.Type != CouponType.BonusMultiplier && coupon.CouponBonusMultiplier != null)
            {
                _integrationDataContext.CouponBonusMultipliers.Remove(coupon.CouponBonusMultiplier);
            }

            if (coupon.Type != CouponType.DiscountPercent && coupon.CouponDiscountPercent != null)
            {
                _integrationDataContext.CouponDiscountPercents.Remove(coupon.CouponDiscountPercent);
            }

            if (coupon.Type != CouponType.DiscountAmount && coupon.CouponDiscountAmount != null)
            {
                _integrationDataContext.CouponDiscountAmounts.Remove(coupon.CouponDiscountAmount);
            }

            if (coupon.Type != CouponType.ProductFixedPrice && coupon.CouponProductFixedPrice != null)
            {
                _integrationDataContext.CouponProductFixedPrices.Remove(coupon.CouponProductFixedPrice);
            }
        }

        public async Task UpdateAsync(UpdateCouponDTO model)
        {
            var coupon = await GetAdminQuery()
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (coupon == null)
            {
                throw new ArgumentException("Ваучер не знайден");
            }

            if (coupon.Status != CouponStatus.Inactive)
            {
                throw new ArgumentException("Можна редагувати тільки неактивний купон");
            }

            if (model.Class == CouponClass.Common && !model.EmissionBy.HasValue)
            {
                throw new ArgumentException("Оберіть розподілення по еміссії");
            }

            if (model.Class == CouponClass.Personal && model.EmissionBy.HasValue)
            {
                throw new ArgumentException("Розподілення по еміссії доступнє тільки для загальних купонів");
            }

            foreach (var storeId in model.StoreIds)
            {
                if (!await _appUnitOfWork.Stores.Any(x => x.Id == storeId))
                {
                    throw new ArgumentException($"Помилка з id магазину {storeId}");
                }
            }

            coupon.Emission = model.Emission;
            coupon.EmissionBy = model.EmissionBy;
            coupon.Class = model.Class;
            coupon.Image = model.Image;
            coupon.UseTimes = model.UseTimes;

            model.TargetIds ??= new List<int>();

            if ((model.Class == CouponClass.Common) != model.TargetIds.Any())
            {
                throw new ArgumentException("Помилка в таргеті або типі");
            }

            coupon.Targets.Clear();
            coupon.Targets.AddRange(model.TargetIds.Select(x => new CouponToTarget
            {
                TargetId = x
            }));

            var storeCodes = await GetStoreCodesThrowIfNotExistAsync(model.StoreIds);

            coupon.StoreCodes.RemoveAll(x => !storeCodes.Contains(x.StoreCode));
            var storeCodesToAdd = storeCodes
                .Except(coupon.StoreCodes.Select(x => x.StoreCode))
                .Select(x => new CouponToStore { StoreCode = x })
                .ToList();
            coupon.StoreCodes.AddRange(storeCodesToAdd);

            coupon.Name = model.Name;
            coupon.PrivateName = model.PrivateName;

            coupon.Description = model.Description;
            coupon.PrivateDescription = model.PrivateDescription;

            coupon.StartDate = model.StartDate;
            coupon.ExpirationDate = model.ExpirationDate;

            var oldDetails = GetCouponDetailsDTO(coupon);

            await SetDetailsAsync(model.Details, coupon);
            var newDetails = model.Details;

            var logHistory = await _integrationDataContext.SaveChangesAsyncWithLogHistory();
            LogMessage? updatedCouponLogMessage = logHistory.FirstOrDefault(x => x.EntityType == "Coupons" && x.EntityId == coupon.Id);

            if (updatedCouponLogMessage is not null)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                var updatedTo = !string.IsNullOrEmpty(updatedCouponLogMessage.UpdatedTo)
                    ? JsonConvert.DeserializeObject<CouponLogHistoryDTO>(updatedCouponLogMessage.UpdatedTo, settings)
                    : new CouponLogHistoryDTO();
                var updatedFrom = !string.IsNullOrEmpty(updatedCouponLogMessage.UpdatedFrom)
                    ? JsonConvert.DeserializeObject<CouponLogHistoryDTO>(updatedCouponLogMessage.UpdatedFrom, settings)
                    : new CouponLogHistoryDTO();
                updatedFrom.CouponDetails = oldDetails;
                updatedTo.CouponDetails = newDetails;
                updatedCouponLogMessage.UpdatedFrom = JsonConvert.SerializeObject(updatedFrom, settings);
                updatedCouponLogMessage.UpdatedTo = JsonConvert.SerializeObject(updatedTo, settings);
            }

            if (logHistory.Any())
            {
                await _logHistoryService.AddRangeAsync(logHistory);
            }
        }


        public async Task<AdminCouponDTO> GetByAdminAsync(int id)
        {
            var coupon = await GetAdminQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (coupon == null)
            {
                throw new ArgumentException("Ваучер не знайден");
            }

            var dto = await ToAdminCouponDTOAsync(coupon);

            dto.DistributedCount = await _integrationDataContext.CustomerCoupons.Where(x => x.CouponId == coupon.Id).CountAsync();
            dto.UsedCount = await _integrationDataContext.CustomerCoupons.Where(x => x.CouponId == coupon.Id && coupon.UseTimes == x.UsedTimes).CountAsync();
            
            if (dto.Status == CouponStatus.Active && dto.ExpirationDate > DateTime.UtcNow)
            {
                dto.AvailableCount = coupon.Class == CouponClass.Common
                    ? dto.DistributedCount - dto.UsedCount
                    : dto.Emission - dto.UsedCount;
            }

            return dto;
        }

        private async Task<AssignedCustomerCouponDTO> ToAssignedCustomerCouponDTOAsync(CustomerCoupon customerCoupon)
        {
            var storeIds = await _appUnitOfWork.Stores.GetAllStores()
                .Where(store => customerCoupon.Coupon.StoreCodes.Select(sc => sc.StoreCode).Contains(store.Number))
                .Select(store => store.Id)
                .ToListAsync();

            return new AssignedCustomerCouponDTO
            {
                Activated = customerCoupon.Activated,
                Type = customerCoupon.Coupon.Type,
                Description = customerCoupon.Coupon.Description,
                Reward = ToRewardDTO(customerCoupon.Coupon),
                ExpirationDate = customerCoupon.Coupon.ExpirationDate,
                Id = customerCoupon.CouponId,
                Name = customerCoupon.Coupon.Name,
                StartDate = customerCoupon.Coupon.StartDate,
                StoreIds = storeIds,
                CustomerCouponId = customerCoupon.Id,
                RemainingTimes = customerCoupon.Coupon.UseTimes - customerCoupon.UsedTimes,
                Image = customerCoupon.Coupon.Image,
            };
        }

        public static (CouponRewardShortInfo, string) ToReward(CouponDetailsDTO coupon, Product? product)
        {
            if (coupon is null)
            {
                return (new CouponRewardShortInfo(), null);
            }

            switch (coupon)
            {
                case CouponProductFixedPriceDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.Price.ToString(),
                        },
                        $"{x.Price:F2} грн (фіксована ціна)");
                case CouponDiscountAmountDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.Amount.ToString(),
                        },
                        $"-{x.Amount:F2} грн");
                case CouponDiscountPercentDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.Percent.ToString(),
                        },
                        $"-{x.Percent:F2} %");
                case CouponBonusMultiplierDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.Multiplier.ToString(),
                        },
                        $"x{x.Multiplier:F2} бонуси");
                case CouponAdditionalBonusDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.Bonus.ToString(),
                        Product = product is null ? null : new ProductShortInfoDTO
                        {
                            Code = product?.Code,
                            Title = product?.Title,
                            Image = product?.ImagePath
                        },
                        },
                    product is null ? $"+{x.Bonus:F2} бонуси" : $"{product.Title} +{x.Bonus:F2} бонуси");
                case CouponCombinationFixedPriceDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.FixedPrice.ToString(),
                        },
                        $"{x.FixedPrice:F2} грн");
                case CouponCombinationPriceDiscountDTO x:
                    return (new CouponRewardShortInfo
                        {
                            Value = x.Price.ToString(),
                            Product = new ProductShortInfoDTO
                            {
                                Code = product?.Code,
                                Title = product?.Title,
                                Image = product?.ImagePath
                            },
                            Quantity = x.Quantity,

                        },
                        $"{product?.Title} - {x.Price:F2} грн");
            }

            return(new CouponRewardShortInfo(), null);
        }

        private static CouponRewardShortInfo ToRewardDTO(Coupon coupon)
            => coupon.Type switch
            {
                CouponType.AdditionalBonus => new CouponRewardShortInfo
                {
                    Value = coupon.CouponAdditionalBonus.Bonus.ToString(),
       
                },
                CouponType.CombinationPriceDiscount => new CouponRewardShortInfo
                {
                    Value = coupon.CouponCombinationPriceDiscount.Price.ToString(),
                    Quantity = coupon.CouponCombinationPriceDiscount.Quantity,
                    Product = new ProductShortInfoDTO
                    {
                        Code = coupon.CouponCombinationPriceDiscount.Product.Code,
                        Title = coupon.CouponCombinationPriceDiscount.Product.Title,
                        Image = coupon.CouponCombinationPriceDiscount.Product.ImagePath,
                    },
                },
                CouponType.CombinationFixedPrice => new CouponRewardShortInfo
                {
                    Value = coupon.CouponCombinationFixedPrice.FixedPrice.ToString(),
                },
                CouponType.DiscountPercent => new CouponRewardShortInfo
                {
                    Value = coupon.CouponDiscountPercent.Percent.ToString(),
                },
                CouponType.DiscountAmount => new CouponRewardShortInfo
                {
                    Value = coupon.CouponDiscountAmount.Amount.ToString(),
                },
                CouponType.BonusMultiplier => new CouponRewardShortInfo
                {
                    Value = coupon.CouponBonusMultiplier.Multiplier.ToString(),
                    //Product = coupon.CouponBonusMultiplier.Product is null ? null : new ProductShortInfoDTO
                    //{
                    //    Code = coupon.CouponBonusMultiplier.Product.Code,
                    //    Title = coupon.CouponBonusMultiplier.Product.Title,
                    //    Image = coupon.CouponBonusMultiplier.Product.ImagePath,
                    //},
                },
                CouponType.ProductFixedPrice => new CouponRewardShortInfo
                {
                    Value = coupon.CouponProductFixedPrice.Price.ToString(),
                    Product = new ProductShortInfoDTO
                    {
                        Code = coupon.CouponProductFixedPrice.Product.Code,
                        Title = coupon.CouponProductFixedPrice.Product.Title,
                        Image = coupon.CouponProductFixedPrice.Product.ImagePath,
                    },
                    Quantity = coupon.CouponProductFixedPrice.Quanitity,
                },
            };


        private async Task<AdminCouponDTO> ToAdminCouponDTOAsync(Coupon coupon)
        {
            var storeCodes = coupon.StoreCodes.Select(sc => sc.StoreCode).ToList();
            var storeIds = await _appUnitOfWork.Stores.Find(store => storeCodes.Contains(store.Number))
                .Select(store => store.Id)
                .ToListAsync();

            return new AdminCouponDTO
            {
                Type = coupon.Type,
                Class = coupon.Class,
                CreatedAt = coupon.CreatedAt,
                Description = coupon.Description,
                Details = GetCouponDetailsDTO(coupon),
                ExpirationDate = coupon.ExpirationDate,
                EmissionBy = coupon.EmissionBy,
                Id = coupon.Id,
                Status = coupon.Status,
                Name = coupon.Name,
                PrivateName = coupon.PrivateName,
                PrivateDescription = coupon.PrivateDescription,
                StartDate = coupon.StartDate,
                StoreIds = storeIds,
                TargetIds = coupon.Targets.Select(x => x.TargetId).ToList(),
                UpdatedAt = coupon.UpdatedAt,
                UseTimes = coupon.UseTimes,
                Emission = coupon.Emission,
                Image = coupon.Image,
                Reward = ToRewardDTO(coupon),
            };
        }

        public async Task<PagedListDTO<AdminCouponDTO>> GetCouponsByAdminAsync(
            PagingDTO pagingDTO,
            AdminCouponFilterDTO filter)
        {
            var query = GetAdminQuery();

            if (!string.IsNullOrEmpty(filter.searchParam))
            {
                query = query.Where(x => x.Name.Contains(filter.searchParam) || x.PrivateName.Contains(filter.searchParam));
            }

            if (filter.branchId.HasValue)
            {
                var storeCodes = await _appUnitOfWork.Stores.Find(x => x.BranchId == filter.branchId.Value)
                    .Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.StoreCodes.Any(c => storeCodes.Contains(c.StoreCode)));
            }

            if (filter.date.HasValue)
            {
                var dateTime = filter.date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.StartDate >= dateTimeFrom && x.StartDate < dateTimeTo);
            }

            if (filter.couponClass.HasValue)
            {
                query = query.Where(x => x.Class == filter.couponClass.Value);
            }

            if (filter.status.HasValue)
            {
                query = query.Where(x => x.Status == filter.status.Value);
            }

            if (filter.startsEarlierThan.HasValue)
            {
                query = query.Where(x => x.StartDate <= filter.startsEarlierThan.Value);
            }

            if (filter.expiresLaterThan.HasValue)
            {
                query = query.Where(x => x.ExpirationDate >= filter.expiresLaterThan.Value);
            }

            var results = await query.GetPagedListAsync(pagingDTO, ToAdminCouponDTOAsync);

            var couponIds = results.Items.Select(c => c.Id).ToList();

            var distributedCounts = await _integrationDataContext.CustomerCoupons
                .Where(x => couponIds.Contains(x.CouponId))
                .GroupBy(x => x.CouponId)
                .Select(g => new { CouponId = g.Key, Count = g.Count() })
                .ToListAsync();

            var usedCounts = await _integrationDataContext.CustomerCoupons
                .Where(x => couponIds.Contains(x.CouponId) && x.UsedTimes == x.Coupon.UseTimes)
                .GroupBy(x => x.CouponId)
                .Select(g => new { CouponId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var coupon in results.Items)
            {
                coupon.DistributedCount = distributedCounts.FirstOrDefault(x => x.CouponId == coupon.Id)?.Count ?? 0;
                coupon.UsedCount = usedCounts.FirstOrDefault(x => x.CouponId == coupon.Id)?.Count ?? 0;
                if (coupon.Status == CouponStatus.Active && coupon.ExpirationDate > DateTime.UtcNow)
                {
                    coupon.AvailableCount = coupon.Class == CouponClass.Common
                        ? coupon.DistributedCount - coupon.UsedCount
                        : coupon.Emission - coupon.UsedCount;
                }
            }

            return results;
        }

        private static CouponDetailsDTO GetCouponDetailsDTO(Coupon coupon)
        {
            CouponDetailsDTO details = null;
            switch (coupon.Type)
            {
                case CouponType.CombinationPriceDiscount:
                    {
                        var activators = new List<CouponActivatorDTO>();

                        foreach (var activator in coupon.CouponCombinationPriceDiscount.ProductActivators)
                        {
                            activators.Add(new CouponProductActivatorDTO
                            {
                                ProductCode = activator.ProductCode,
                                Quantity = activator.Quantity,
                            });
                        }

                        foreach (var activator in coupon.CouponCombinationPriceDiscount.SupplierActivators)
                        {
                            activators.Add(new CouponSupplierActivatorDTO
                            {
                                SupplierCode = activator.SupplierCode,
                            });
                        }

                        foreach (var activator in coupon.CouponCombinationPriceDiscount.ManufacturerActivators)
                        {
                            activators.Add(new CouponManufacturerActivatorDTO
                            {
                                ManufacturerCode = activator.ManufacturerCode,
                            });
                        }

                        foreach (var activator in coupon.CouponCombinationPriceDiscount.CategoryActivators)
                        {
                            activators.Add(new CouponCategoryActivatorDTO
                            {
                                CategoryCode = activator.CategoryCode,
                            });
                        }

                        foreach (var activator in coupon.CouponCombinationPriceDiscount.BrandActivators)
                        {
                            activators.Add(new CouponBrandActivatorDTO
                            {
                                BrandCode = activator.BrandCode,
                            });
                        }

                        details = new CouponCombinationPriceDiscountDTO
                        {
                            Activators = activators,
                            AllRequired = coupon.CouponCombinationPriceDiscount.AllRequired,
                            Compensation = coupon.CouponCombinationPriceDiscount.Compensation,
                            Price = coupon.CouponCombinationPriceDiscount.Price,
                            SupplierCode = coupon.CouponCombinationPriceDiscount.SupplierCode,
                            ProductCode = coupon.CouponCombinationPriceDiscount.ProductCode,
                            Quantity = coupon.CouponCombinationPriceDiscount.Quantity,
                        };

                        break;
                    }
                case CouponType.CombinationFixedPrice:
                    {
                        details = new CouponCombinationFixedPriceDTO
                        {
                            Activators = coupon.CouponCombinationFixedPrice.Activators
                                .Select(x => new CouponProductActivatorDTO
                                {
                                    ProductCode = x.ProductCode,
                                    Quantity = x.Quantity,
                                }).ToList(),
                            FixedPrice = coupon.CouponCombinationFixedPrice.FixedPrice,
                        };

                        break;
                    }
                case CouponType.AdditionalBonus: 
                    {
                        details = new CouponAdditionalBonusDTO
                        {
                            Bonus = coupon.CouponAdditionalBonus.Bonus,
                        };

                        break;
                    }
                case CouponType.BonusMultiplier:
                    {
                        details = new CouponBonusMultiplierDTO
                        {
                            Multiplier = coupon.CouponBonusMultiplier.Multiplier,
                            Activators = new List<CouponActivatorDTO>()
                        };

                        foreach (var activator in coupon.CouponBonusMultiplier.ProductActivators)
                        {
                            ((CouponBonusMultiplierDTO)details).Activators.Add(new CouponProductActivatorDTO
                            {
                                ProductCode = activator.ProductCode,
                                Quantity = activator.Quantity,
                            });
                        }
                        foreach (var activator in coupon.CouponBonusMultiplier.SupplierActivators)
                        {
                            ((CouponBonusMultiplierDTO)details).Activators.Add(new CouponSupplierActivatorDTO
                            {
                                SupplierCode = activator.SupplierCode,
                            });
                        }
                        foreach (var activator in coupon.CouponBonusMultiplier.ManufacturerActivators)
                        {
                            ((CouponBonusMultiplierDTO)details).Activators.Add(new CouponManufacturerActivatorDTO
                            {
                                ManufacturerCode = activator.ManufacturerCode,
                            });
                        }
                        foreach (var activator in coupon.CouponBonusMultiplier.CategoryActivators)
                        {
                            ((CouponBonusMultiplierDTO)details).Activators.Add(new CouponCategoryActivatorDTO
                            {
                                CategoryCode = activator.CategoryCode,
                            });
                        }
                        foreach (var activator in coupon.CouponBonusMultiplier.BrandActivators)
                        {
                            ((CouponBonusMultiplierDTO)details).Activators.Add(new CouponBrandActivatorDTO
                            {
                                BrandCode = activator.BrandCode,
                            });
                        };

                        break;
                    }
                case CouponType.DiscountPercent:
                    {
                        details = new CouponDiscountPercentDTO
                        {
                            Percent = coupon.CouponDiscountPercent.Percent,
                        };

                        break;
                    }
                case CouponType.DiscountAmount:
                    {
                        details = new CouponDiscountAmountDTO
                        {
                            Amount = coupon.CouponDiscountAmount.Amount,
                        };

                        break;
                    }
                case CouponType.ProductFixedPrice:
                    {
                        details = new CouponProductFixedPriceDTO
                        {
                            Price = coupon.CouponProductFixedPrice.Price,
                            ProductCode = coupon.CouponProductFixedPrice.ProductCode,
                            Quanitity = coupon.CouponProductFixedPrice.Quanitity,
                        };

                        break;
                    }
            }

            return details;
        }

        public async Task SetActiveCustomerCouponAsync(int customerCouponId, bool activeFlag)
        {
            var currentDate = DateTime.UtcNow.SetKindUtc();
            var query = await GetBaseCustomerQueryAsync(_userContext.CustomerId!.Value, (byte)_userContext.BranchId!.Value);
            var coupon = await query.FirstOrDefaultAsync(x => x.Id == customerCouponId);

            if (coupon == null)
            {
                throw new ArgumentException("Немає такого купон або він вже використанний");
            }

            coupon.Activated = activeFlag;

            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task SetAllCustomerCouponsAsActiveAsync(bool activeFlag, int? storeId = null)
        {
            var currentDate = DateTime.UtcNow.SetKindUtc();
            var coupons = await GetBaseCustomerQueryAsync(_userContext.CustomerId!.Value, (byte)_userContext.BranchId!.Value);

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                coupons = coupons.Where(x => x.Coupon.StoreCodes.Any(x => x.StoreCode == storeCode));
            }

            await coupons.ForEachAsync(x => x.Activated = activeFlag);
            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task<int> AssignPersonalCouponToCustomerAsync(int couponId, int customerId)
        {
            var utcNow = DateTime.UtcNow;

            if (!await _integrationDataContext.Coupons
                .Where(x => x.Id == couponId && x.Class == CouponClass.Personal
                    && x.Status == CouponStatus.Active
                    && x.StartDate <= utcNow && x.ExpirationDate >= utcNow
                    && x.Emission - x.CustomerCoupons.Count > 0)
                .AnyAsync())
            {
                throw new ArgumentException($"Купон з id {couponId} не знайден/неактивований/недоступний/або кількість використань менша за бажану");
            }

            var customerCoupon = new CustomerCoupon
            {
                CustomerId = customerId,
                CouponId = couponId,
                AssignedAt = utcNow,
            };

            await _integrationDataContext.CustomerCoupons.AddAsync(customerCoupon);

            await _integrationDataContext.SaveChangesAsync();

            return customerCoupon.Id;
        }

        public async Task<CustomerCouponCountDTO> CountForCustomerAsync(int? storeId = null)
        {
            var result = new CustomerCouponCountDTO();
            var query = await GetBaseCustomerQueryAsync(_userContext.CustomerId!.Value, (byte) _userContext.BranchId!.Value);

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                query = query.Where(x => x.Coupon.StoreCodes.Any(x => x.StoreCode == storeCode));
            }

            result.TotalCount = await query.CountAsync();
            result.ActiveCount = await query.Where(x => x.Activated).CountAsync();
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _integrationDataContext.Coupons.FirstOrDefaultAsync(x => x.Id == id
                && !x.IsCardCoupon);
            if (coupon == null)
            {
                throw new ArgumentException("Купон не знайдений");
            }

            if (coupon.Status != CouponStatus.Inactive)
            {
                throw new ArgumentException("Можливо видалити тільки неактивний купон");
            }

            _integrationDataContext.Coupons.Remove(coupon);
            
            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task<PagedListDTO<CustomerCouponForAdminDTO>> GetCustomerCouponsByAdminAsync(int customerId, PagingDTO pagingDTO, RangeDTO<DateTime>? startDate = null, RangeDTO<DateTime>? expirationDate = null, CustomerCouponStatus? status = null)
        {
            var query = _integrationDataContext.CustomerCoupons
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.Product)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.ProductActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.BrandActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationFixedPrice)
                        .ThenInclude(x => x.Activators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.ProductActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.BrandActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponDiscountPercent)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponDiscountAmount)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponProductFixedPrice)
                        .ThenInclude(x => x.Product)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.StoreCodes)
                .Where(x => x.CustomerId == customerId)
                .AsQueryable();

            if (startDate != null)
            {
                query = query.Where(x => x.Coupon.StartDate >= startDate.From && x.Coupon.StartDate <= startDate.To);
            }

            if (expirationDate != null)
            {
                query = query.Where(x => x.Coupon.ExpirationDate >= expirationDate.From && x.Coupon.ExpirationDate <= expirationDate.To);
            }

            if (status.HasValue)
            {
                if (status == CustomerCouponStatus.Expired)
                {
                    query = query.Where(x => x.UsedTimes != x.Coupon.UseTimes && (x.Coupon.Status == CouponStatus.Archived && x.Coupon.UpdatedAt >= x.Coupon.ExpirationDate) && x.Coupon.ExpirationDate <= DateTime.UtcNow);
                }

                if (status == CustomerCouponStatus.Inactive)
                {
                    query = query.Where(x => x.UsedTimes != x.Coupon.UseTimes
                        && !x.Activated
                        && x.Coupon.Status == CouponStatus.Active
                        && x.Coupon.StartDate >= DateTime.UtcNow
                        && x.Coupon.ExpirationDate <= DateTime.UtcNow);
                }

                if (status == CustomerCouponStatus.Used)
                {
                    query = query.Where(x => x.UsedTimes == x.Coupon.UseTimes);
                }

                if (status == CustomerCouponStatus.Active)
                {
                    query = query.Where(x => x.Activated 
                        && x.Coupon.Status == CouponStatus.Active
                        && x.Coupon.StartDate <= DateTime.UtcNow
                        && x.Coupon.ExpirationDate >= DateTime.UtcNow
                    );
                }

                if (status == CustomerCouponStatus.Archived)
                {
                    query = query.Where(x => x.UsedTimes != x.Coupon.UseTimes && (x.Coupon.UpdatedAt <= x.Coupon.ExpirationDate) && x.Coupon.Status == CouponStatus.Archived);
                }
            }

            return await query.GetPagedListAsync(pagingDTO, ToCustomerCouponForAdminDTOAsync);
        }

        private async Task<CustomerCouponForAdminDTO> ToCustomerCouponForAdminDTOAsync(CustomerCoupon entity)
        {
            var storeIds = await _appUnitOfWork.Stores.GetAllStores()
                .Where(store => entity.Coupon.StoreCodes.Select(sc => sc.StoreCode).Contains(store.Number))
                .Select(store => store.Id)
                .ToListAsync();

            return new CustomerCouponForAdminDTO
            {
                Activated = entity.Activated,
                Type = entity.Coupon.Type,
                Description = entity.Coupon.Description,
                Reward = ToRewardDTO(entity.Coupon),
                ExpirationDate = entity.Coupon.ExpirationDate,
                Id = entity.CouponId,
                Name = entity.Coupon.Name,
                StartDate = entity.Coupon.StartDate,
                CustomerCouponId = entity.Id,
                RemainingTimes = entity.Coupon.UseTimes - entity.UsedTimes,
                Image = entity.Coupon.Image,
                Status = GetStatusFromEntity(entity),
                StoreIds = storeIds 
            };
        }

        private static CustomerCouponStatus GetStatusFromEntity(CustomerCoupon entity)
        {
            if (entity.UsedTimes == entity.Coupon.UseTimes)
            {
                return CustomerCouponStatus.Used;
            }

            if (entity.Coupon.ExpirationDate < DateTime.UtcNow && entity.Coupon.UpdatedAt >= entity.Coupon.ExpirationDate)
            {
                return CustomerCouponStatus.Expired;
            }

            if (entity.UsedTimes != entity.Coupon.UseTimes && (entity.Coupon.UpdatedAt <= entity.Coupon.ExpirationDate) && entity.Coupon.Status == CouponStatus.Archived)
            {
                return CustomerCouponStatus.Archived;
            }

            return entity.Activated ? CustomerCouponStatus.Active : CustomerCouponStatus.Inactive;
        }

        public async Task<CustomerCouponForAdminDTO> GetCustomerCouponByAdminAsync(int customerCouponId)
        {
            var coupon = await _integrationDataContext.CustomerCoupons
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.Product)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.ProductActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.BrandActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                        .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationPriceDiscount)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponCombinationFixedPrice)
                        .ThenInclude(x => x.Activators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.ProductActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.CategoryActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.BrandActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.SupplierActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                        .ThenInclude(x => x.ManufacturerActivators)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponBonusMultiplier)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponDiscountPercent)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponDiscountAmount)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.CouponProductFixedPrice)
                        .ThenInclude(x => x.Product)
                .Include(x => x.Coupon)
                    .ThenInclude(x => x.StoreCodes)
                .FirstOrDefaultAsync(x => x.Id == customerCouponId);
            
            if (coupon == null)
            {
                throw new ArgumentException("Купон не знайден");
            }

            return await ToCustomerCouponForAdminDTOAsync(coupon);
        }

        public async Task<List<AssignedCustomerCouponDTO>> GetAllCustomerCouponsByCustomerId(int customerId, byte branchId)
        {
            var query = await GetBaseCustomerQueryAsync(customerId, branchId);
            query = query.Where(x => x.Activated);
            var entities = await query.ToListAsync();
            var dtos = new List<AssignedCustomerCouponDTO>();
            
            foreach (var entity in entities)
            {
                dtos.Add(await ToAssignedCustomerCouponDTOAsync(entity));
            }

            return dtos;
        }

        public async Task UseCouponsAsync(int customerId, HashSet<int> customerCouponIds)
        {
            foreach (var customerCouponId in customerCouponIds)
            {
                var coupon = await _integrationDataContext.CustomerCoupons
                    .Include(x => x.Coupon)
                    .FirstOrDefaultAsync(x => x.Id == customerCouponId && x.CustomerId == customerId);

                if (coupon == null)
                {
                    throw new ArgumentException("Купон не знайден");
                }

                if (!(coupon.Coupon.Status == CouponStatus.Active && coupon.Coupon.StartDate <= DateTime.UtcNow && coupon.Coupon.ExpirationDate >= DateTime.UtcNow))
                {
                    throw new ArgumentException("Купон більше недоступний");
                }

                if (!coupon.Activated)
                {
                    throw new ArgumentException("Купон не активований");
                }

                if (coupon.UsedTimes >= coupon.Coupon.UseTimes)
                {
                    throw new ArgumentException("Купон використаний");
                }

                coupon.UsedTimes++;

                if (coupon.UsedTimes == coupon.Coupon.UseTimes)
                {
                    coupon.UsedAt = DateTime.UtcNow;
                }
            }

            await _integrationDataContext.SaveChangesAsync();
        }

        private async Task<List<string>> GetStoreCodesThrowIfNotExistAsync(List<int> storeIds)
        {
            if (storeIds == null || !storeIds.Any())
            {
                throw new ArgumentException("Оберіть хоча б 1 магазин");
            }

            var storeCodes = new List<string>();

            foreach (var storeId in storeIds)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstOrDefaultAsync();
                if (storeCode == null)
                {
                    throw new ArgumentException($"Магазин з id {storeId} не знайден.");
                }

                storeCodes.Add(storeCode);
            }

            return storeCodes;
        }
    }
}
