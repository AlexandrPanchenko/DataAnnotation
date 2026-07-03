using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.AccumulationCard;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.LogHistory;
using JetFlight.Shared.Models.Product;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace JetFlight.Service.Services
{
    public interface IAccumulationCardService
    {
        Task<AdminAccumulationCardDTO> CreateAsync(CreateAccumulationCardDTO model);
        Task<AdminAccumulationCardDTO> GetAdminCardByIdAsync(int id);
        Task<PagedListDTO<AdminAccumulationCardDTO>> GetAdminCardsAsync(
            PagingDTO pagingDTO,
            string? searchParam = null,
            byte? branchId = null,
            int? cityId = null,
            int? storeId = null,
            DateOnly? date = null,
            AccumulationCardStatus? status = null);
        Task UpdateAsync(UpdateAccumulationCardDTO model);
        Task<CustomerAccumulationCardDTO> GetCustomerCardByIdAsync(int customerAccumulationCardId);
        Task<PagedListDTO<CustomerAccumulationCardDTO>> GetCustomerCardsAsync(PagingDTO pagingDTO, int? storeId = null, CustomerAccumulationCardStatus? status = null);

        Task SetCustomerCardActiveFlagAsync(int customerAccumulationCardId, bool isActiveFlag);
        Task SetApplicableCardsActiveFlagAsync(bool activeFlag, int? storeId = null);
        Task DeleteAsync(int id);

        Task PublishAsync(int id);
        Task ArchiveAsync(int id);

        Task CompleteAsync(int id, int rewardCouponId);

        Task<CustomerAccumulationCardCountDTO> CountForCustomerAsync(int? storeId = null);

        Task<PagedListDTO<CustomerAccumulationCardForAdminDTO>> GetCustomerCardsByAdminAsync(int customerId, PagingDTO pagingDTO, RangeDTO<DateTime>? startDate = null, RangeDTO<DateTime>? expirationDate = null, CustomerAccumulationCardStatusForAdmin? status = null);

    }

    public class AccumulationCardService : IAccumulationCardService
    {
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly IDataUnitOfWork _appUnitOfWork;
        private readonly ITargetService _targetService;
        private readonly IUserContext _userContext;
        private readonly IJobSchedulerService _jobSchedulerService;
        private readonly ILogHistoryService _logHistoryService;

        public AccumulationCardService(
            IntegrationDataContext integrationDataContext,
            IDataUnitOfWork appUnitOfWork,
            ITargetService targetService,
            IUserContext userContext,
            IJobSchedulerService jobSchedulerService,
            ILogHistoryService logHistoryService)
        {
            _integrationDataContext = integrationDataContext;
            _appUnitOfWork = appUnitOfWork;
            _targetService = targetService;
            _userContext = userContext;
            _jobSchedulerService = jobSchedulerService;
            _logHistoryService = logHistoryService;
        }

        public async Task<AdminAccumulationCardDTO> CreateAsync(CreateAccumulationCardDTO model)
        {
            var accumulationCard = new AccumulationCard
            {
                CountToComplete = model.CountToComplete,
                AllRequired = model.AllRequired,
                Description = model.Description,
                Name = model.Name,
                Targets = model.TargetIds.Select(x => new AccumulationCardToTarget
                {
                    TargetId = x
                }).ToList(),
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                Icon = model.Icon,
                Status = AccumulationCardStatus.Inactive,
            };

            accumulationCard.Coupons = await CreateCouponsOrThrowIfInvalidAsync(
                model.ProductCodes,
                accumulationCard,
                model.Image,
                model.CouponDescription,
                model.StartDate,
                model.ExpirationDate,
                model.StoreIds);

            await _integrationDataContext.AccumulationCards.AddAsync(accumulationCard);
            await _integrationDataContext.SaveChangesAsync(ignoreLogs: true, cancellationToken: CancellationToken.None);

            var logMessage = new LogMessage
            {
                AdminId = _userContext.AdminId,
                EntityType = "AccumulationCards",
                UpdatedFrom = null,
                UpdatedTo = JsonConvert.SerializeObject(new AccumulationCard
                {
                    Id = accumulationCard.Id,
                    Name = accumulationCard.Name,
                    Icon = accumulationCard.Icon,
                    CountToComplete = accumulationCard.CountToComplete,
                    AllRequired = accumulationCard.AllRequired,
                    Description = accumulationCard.Description,
                    Status = accumulationCard.Status,
                    CreatedAt = accumulationCard.CreatedAt,
                },
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    }),
                EntityId = accumulationCard.Id,
                Action = ActionConstant.Inserted,
                Date = DateTime.UtcNow,
            };

            await _logHistoryService.AddAsync(logMessage);

            return await ToAdminDTOAsync(new AccumulationCardAndCouponItem { AccumulationCard = accumulationCard, Coupon =  accumulationCard.Coupons.First()});
        }

        private async Task<AdminAccumulationCardDTO> ToAdminDTOAsync(AccumulationCardAndCouponItem item)
        {
            var storeIds = await _appUnitOfWork.Stores.GetAllStores()
                .Where(store => item.Coupon.StoreCodes.Select(sc => sc.StoreCode).Contains(store.Number))
                .Select(store => store.Id)
                .ToListAsync();

            return new AdminAccumulationCardDTO
            {
                AllRequired = item.AccumulationCard.AllRequired,
                CountToComplete = item.AccumulationCard.CountToComplete,
                Description = item.AccumulationCard.Description,
                CouponDescription = item.Coupon.Description,
                Image = item.Coupon.Image,
                Icon = item.AccumulationCard.Icon,
                Id = item.AccumulationCard.Id,
                Status = item.AccumulationCard.Status,
                Name = item.AccumulationCard.Name,
                ExpirationDate = item.Coupon.ExpirationDate,
                StartDate = item.Coupon.StartDate,
                TargetIds = item.AccumulationCard.Targets.Select(x => x.TargetId).ToList(),
                ProductCodes = item.AccumulationCard.Coupons.Select(x => x.CouponProductFixedPrice.ProductCode).ToList(),
                Products = item.AccumulationCard.Coupons.Select(x => new ProductShortInfoDTO
                {
                    Code = x.CouponProductFixedPrice.Product.Code,
                    Title = x.CouponProductFixedPrice.Product.Title,
                    Image = x.CouponProductFixedPrice.Product.ImagePath,
                }).ToList(),
                StoreIds = storeIds,
            };
        }

        public async Task<PagedListDTO<AdminAccumulationCardDTO>> GetAdminCardsAsync(
            PagingDTO pagingDTO,
            string? searchParam = null,
            byte? branchId = null,
            int? cityId = null,
            int? storeId = null,
            DateOnly? date = null,
            AccumulationCardStatus? status = null)
        {
            var query = _integrationDataContext.AccumulationCards
                .Include(x => x.Coupons)
                    .ThenInclude(x => x.CouponProductFixedPrice)
                        .ThenInclude(x => x.Product)
                .Include(x => x.Coupons)
                    .ThenInclude(x => x.StoreCodes)
                .Include(x => x.Targets)
                .Select(x => new AccumulationCardAndCouponItem
                {
                    AccumulationCard = x,
                    Coupon = x.Coupons.First()
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchParam))
            {
                query = query.Where(x => x.AccumulationCard.Name.Contains(searchParam));
            }

            if (branchId.HasValue)
            {
                var storeCodes = await _appUnitOfWork.Stores.Find(x => x.BranchId == branchId.Value)
                    .Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(c => storeCodes.Contains(c.StoreCode)));
            }

            if (cityId.HasValue)
            {
                var storeCodes = await _appUnitOfWork.Stores.Find(x => x.CityId == cityId)
                    .Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(c => storeCodes.Contains(c.StoreCode)));
            }

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                query = query.Where(x => x.Coupon.StoreCodes.Any(c => c.StoreCode == storeCode));
            }

            if (date != null)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.Coupon.StartDate >= dateTimeFrom && x.Coupon.StartDate < dateTimeTo);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.AccumulationCard.Status == status.Value);
            }

            query = query.OrderByDescending(x => x.AccumulationCard.CreatedAt);

            var result = await query.GetPagedListAsync(pagingDTO, ToAdminDTOAsync);
            
            foreach (var card in result.Items)
            {
                var cardQuery = _integrationDataContext.CustomerAccumulationCards.Where(x => x.AccumulationCardId == card.Id);
                card.ActivatedCount = await cardQuery.Where(x => x.Status == CustomerAccumulationCardStatus.Active).CountAsync();
                card.DistributedCount = await cardQuery.CountAsync();
                card.CompletedCount = await cardQuery.Where(x => x.Status == CustomerAccumulationCardStatus.Completed).CountAsync();
            }

            return result;
        }

        public async Task UpdateAsync(UpdateAccumulationCardDTO model)
        {
            var card = await _integrationDataContext
                .AccumulationCards
                .Include(x => x.Coupons)
                .ThenInclude(x => x.CouponProductFixedPrice)
                .Include(x => x.Targets)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            if (card.Status != AccumulationCardStatus.Inactive)
            {
                throw new ArgumentException("Можна редагувати тільки неактивну картку");
            }

            card.CountToComplete = model.CountToComplete;
            card.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            card.Name = model.Name;
            card.Description = model.Description;

            card.Targets.Clear();
            card.Targets.AddRange(model.TargetIds.Select(x => new AccumulationCardToTarget
            {
                TargetId = x
            }));
            card.Icon = model.Icon;

            var oldProducts = card.Coupons.Where(x => x.CouponProductFixedPrice != null)
                .Select(x => x.CouponProductFixedPrice.ProductCode).ToList();

            _integrationDataContext.Coupons.RemoveRange(card.Coupons);
            card.Coupons.Clear();

            var coupons = await CreateCouponsOrThrowIfInvalidAsync(
                model.ProductCodes,
                card,
                model.Image,
                model.CouponDescription,
                model.StartDate,
                model.ExpirationDate,
                model.StoreIds);
            card.Coupons.AddRange(coupons);
            var logHistory = await _integrationDataContext.SaveChangesAsyncWithLogHistory();
            LogMessage? updatedAccumulationCardLogMessage = logHistory.FirstOrDefault(x => x.EntityType == "AccumulationCards" && x.EntityId == card.Id);

            if (updatedAccumulationCardLogMessage is not null)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                var updatedTo = !string.IsNullOrEmpty(updatedAccumulationCardLogMessage.UpdatedTo)
                    ? JsonConvert.DeserializeObject<AccumulationCardLogHistoryDTO>(updatedAccumulationCardLogMessage.UpdatedTo, settings)
                    : new AccumulationCardLogHistoryDTO();
                var updatedFrom = !string.IsNullOrEmpty(updatedAccumulationCardLogMessage.UpdatedFrom)
                    ? JsonConvert.DeserializeObject<AccumulationCardLogHistoryDTO>(updatedAccumulationCardLogMessage.UpdatedFrom, settings)
                    : new AccumulationCardLogHistoryDTO();
                updatedFrom.ProductCodes = oldProducts;
                updatedTo.ProductCodes = model.ProductCodes;
                updatedAccumulationCardLogMessage.UpdatedFrom = JsonConvert.SerializeObject(updatedFrom, settings);
                updatedAccumulationCardLogMessage.UpdatedTo = JsonConvert.SerializeObject(updatedTo, settings);
            }

            if (logHistory.Any())
            {
                await _logHistoryService.AddRangeAsync(logHistory);
            }
        }

        private async Task<List<Coupon>> CreateCouponsOrThrowIfInvalidAsync(
            List<string> productCodes,
            AccumulationCard card,
            string image,
            string couponDescription,
            DateTime startDate,
            DateTime expirationDate,
            List<int> storeIds)
        {
            var storeCodes = await GetStoreCodesThrowIfNotExistAsync(storeIds);

            if (productCodes == null || !productCodes.Any())
            {
                throw new ArgumentException("Оберіть хоча б 1 продукт");
            }

            var result = new List<Coupon>();

            foreach (var productCode in productCodes)
            {
                var product = await _integrationDataContext.Products.FirstOrDefaultAsync(x => x.Code == productCode);
                if (product == null)
                {
                    throw new ArgumentException($"Продукт з кодом {productCode} не знайден.");
                }

                result.Add(new Coupon
                {
                    Class = CouponClass.Personal,
                    CreatedAt = card.CreatedAt,
                    UpdatedAt = card.UpdatedAt,
                    Description = couponDescription,
                    Emission = int.MaxValue,
                    StartDate = startDate,
                    ExpirationDate = expirationDate,
                    StoreCodes = storeCodes.Select(x => new CouponToStore
                    {
                        StoreCode = x,
                    }).ToList(),
                    Image = image,
                    PrivateName = string.Empty,
                    Name = $"{card.Name} (Нагорода): {product.Title}",
                    Status = CouponStatus.Active,
                    IsCardCoupon = true,
                    Type = CouponType.ProductFixedPrice,
                    PrivateDescription = string.Empty,
                    CouponProductFixedPrice = new CouponProductFixedPrice
                    {
                        Price = 1, // fixed price as 1 UAH
                        Quanitity = 1, // 1 product
                        ProductCode = productCode,
                    },
                    UseTimes = 1,
                });
            }

            return result;
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

        public async Task<AdminAccumulationCardDTO> GetAdminCardByIdAsync(int id)
        {
            var card = await _integrationDataContext.AccumulationCards
                .Include(x => x.Coupons)
                    .ThenInclude(x => x.CouponProductFixedPrice)
                        .ThenInclude(x => x.Product)
                .Include(x => x.Coupons)
                    .ThenInclude(x => x.StoreCodes)
                .Include(x => x.Targets)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            var dto = await ToAdminDTOAsync(new AccumulationCardAndCouponItem { AccumulationCard = card, Coupon = card.Coupons.First() });

            var cardQuery = _integrationDataContext.CustomerAccumulationCards.Where(x => x.AccumulationCardId == id);

            dto.ActivatedCount = await cardQuery.Where(x => x.Status == CustomerAccumulationCardStatus.Active).CountAsync();
            dto.DistributedCount = await cardQuery.CountAsync();
            dto.CompletedCount = await cardQuery.Where(x => x.Status == CustomerAccumulationCardStatus.Completed).CountAsync();

            return dto;
        }

        private async Task<IQueryable<CustomerAccumulationCardAndCouponItem>> GetCustomerQueryAsync()
        {
            var storeCodes = await _appUnitOfWork.Stores.Find(x => x.BranchId == (byte) _userContext.BranchId!.Value)
                .Select(x => x.Number).ToListAsync();

            var date = DateTime.UtcNow.SetKindUtc();
            return _integrationDataContext.CustomerAccumulationCards
                .Include(x => x.AccumulationCard)
                    .ThenInclude(x => x.Coupons)
                        .ThenInclude(x => x.CouponProductFixedPrice)
                            .ThenInclude(x => x.Product)
                .Include(x => x.AccumulationCard)
                    .ThenInclude(x => x.Coupons)
                        .ThenInclude(x => x.StoreCodes)
                .Select(x => new CustomerAccumulationCardAndCouponItem
                {
                    CustomerAccumulationCard = x,
                    Coupon = x.AccumulationCard.Coupons.First() 
                })
                .Where(x => x.CustomerAccumulationCard.CustomerId == _userContext.CustomerId.Value
                    && x.Coupon.StoreCodes.Any(x => storeCodes.Contains(x.StoreCode))
                    && x.Coupon.StartDate <= date
                    && x.Coupon.ExpirationDate >= date
                    && x.CustomerAccumulationCard.Status != CustomerAccumulationCardStatus.Completed
                    && x.CustomerAccumulationCard.AccumulationCard.Status == AccumulationCardStatus.Active);
        }

        public async Task<CustomerAccumulationCardDTO> GetCustomerCardByIdAsync(int customerAccumulationCardId)
        {
            var query = await GetCustomerQueryAsync();
            
            var card = await query
                .FirstOrDefaultAsync(x => x.CustomerAccumulationCard.Id ==  customerAccumulationCardId);

            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            return await ToCustomerDTOAsync(card);
        }

        public async Task<PagedListDTO<CustomerAccumulationCardDTO>> GetCustomerCardsAsync(PagingDTO pagingDTO, int? storeId = null, CustomerAccumulationCardStatus? status = null)
        {
            var query = await GetCustomerQueryAsync();

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                query = query.Where(x => x.CustomerAccumulationCard.AccumulationCard.Coupons.First().StoreCodes.Any(x => x.StoreCode == storeCode));
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.CustomerAccumulationCard.Status == status);
            }

            query = query
                .OrderByDescending(x => x.CustomerAccumulationCard.Status)
                .ThenBy(x => x.CustomerAccumulationCard.Id);

            return await query.GetPagedListAsync(pagingDTO, ToCustomerDTOAsync);
        }

        private async Task<CustomerAccumulationCardDTO> ToCustomerDTOAsync(CustomerAccumulationCardAndCouponItem item)
        {
            // Fetch store IDs from the database based on StoreCodes
            var storeIds = await _appUnitOfWork.Stores.GetAllStores()
                .Where(store => item.Coupon.StoreCodes.Select(sc => sc.StoreCode).Contains(store.Number))
                .Select(store => store.Id)
                .ToListAsync();

            return new CustomerAccumulationCardDTO
            {
                Status = item.CustomerAccumulationCard.Status,
                Counter = item.CustomerAccumulationCard.Counter,
                CountToComplete = item.CustomerAccumulationCard.AccumulationCard.CountToComplete,
                Description = item.CustomerAccumulationCard.AccumulationCard.Description,
                CouponDescription = item.Coupon.Description,
                Icon = item.CustomerAccumulationCard.AccumulationCard.Icon,
                CustomerAccumulationCardId = item.CustomerAccumulationCard.Id,
                Id = item.CustomerAccumulationCard.AccumulationCard.Id,
                Image = item.Coupon.Image,
                Name = item.CustomerAccumulationCard.AccumulationCard.Name,
                StartDate = item.Coupon.StartDate,
                ExpirationDate = item.Coupon.ExpirationDate,
                ActivationProducts = item.CustomerAccumulationCard.AccumulationCard.Coupons.Select(x => new ProductShortInfoDTO
                {
                    Code = x.CouponProductFixedPrice.Product.Code,
                    Title = x.CouponProductFixedPrice.Product.Title,
                    Image = x.CouponProductFixedPrice.Product.ImagePath,
                }).ToList(),
                Rewards = item.CustomerAccumulationCard.AccumulationCard.Coupons.Select(x => new AccumulationCardReward
                {
                    CouponId = x.Id,
                    Name = x.Name,
                    Price = x.CouponProductFixedPrice.Price,
                    Product = new ProductShortInfoDTO
                    {
                        Code = x.CouponProductFixedPrice.Product.Code,
                        Title = x.CouponProductFixedPrice.Product.Title,
                        Image = x.CouponProductFixedPrice.Product.ImagePath,
                    },
                }).ToList(),
                StoreIds = storeIds,
            };
        }

        public async Task SetCustomerCardActiveFlagAsync(int customerAccumulationCardId, bool isActiveFlag)
        {
            var query = await GetCustomerQueryAsync();

            var card = await query
                .Select(x => x.CustomerAccumulationCard)
                .FirstOrDefaultAsync(x => x.Id == customerAccumulationCardId);

            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            if (card.Status == CustomerAccumulationCardStatus.Active && card.Counter == card.AccumulationCard.CountToComplete)
            {
                throw new ArgumentException("Картка вже заповнена. Заберіть нагороду");
            }

            card.Status = isActiveFlag ? CustomerAccumulationCardStatus.Active : CustomerAccumulationCardStatus.Inactive;
            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var card = await _integrationDataContext.AccumulationCards
                .Include(x => x.Coupons)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            if (card.Status != AccumulationCardStatus.Inactive)
            {
                throw new ArgumentException("Можна видалити тільки неактивну картку");
            }

            _integrationDataContext.Coupons.RemoveRange(card.Coupons);
            _integrationDataContext.AccumulationCards.Remove(card);

            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            var card = await _integrationDataContext.AccumulationCards
                .Include(x => x.Coupons)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            if (card.Status == AccumulationCardStatus.Archived)
            {
                throw new ArgumentException("Картка вже заархівована");
            }

            var utcNow = DateTime.UtcNow.SetKindUtc();
            card.Status = AccumulationCardStatus.Archived;
            card.UpdatedAt = utcNow;

            foreach (var coupon in card.Coupons)
            {
                coupon.Status = CouponStatus.Archived;
                coupon.UpdatedAt = utcNow;
            }

            await _integrationDataContext.SaveChangesAsync();

            await _jobSchedulerService.RemoveAccumulationCardExpirationJobAsync(card.Id);
        }

        public async Task PublishAsync(int id)
        {
            var card = await _integrationDataContext
                .AccumulationCards
                .Include(x => x.Coupons)
                .Include(x => x.Targets)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (card == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            if (card.Status != AccumulationCardStatus.Inactive)
            {
                throw new ArgumentException("Тільки неактивну картку можно запаблішити");
            }

            if (await _integrationDataContext.AccumulationCards.AnyAsync(x => x.Id == id && x.Coupons.Any(c => c.ExpirationDate <= DateTime.UtcNow)))
            {
                throw new ArgumentException("Змініть дату закінчення на майбутній час, щоб запаблішити картку");
            }

            var connection = _integrationDataContext.Database.GetDbConnection();
            await connection.OpenAsync();
            var createTempTableCommand = connection.CreateCommand();
            createTempTableCommand.CommandText = @"
DROP TABLE IF EXISTS #CustomersForCards;
CREATE TABLE #CustomersForCards
(
Id integer NOT NULL,
);";
            await createTempTableCommand.ExecuteNonQueryAsync();
            var targets = card.Targets.Select(x => x.TargetId).ToList();

            foreach (var t in card.Targets)
            {
                var target = await _targetService.GetTargetEntityAsync(t.TargetId);
                await _targetService.PopulateTargetCustomersIntoTempTableAsync(target, connection);

                var insertTempTableQuery = connection.CreateCommand();
                insertTempTableQuery.CommandText = @"
INSERT INTO #CustomersForCards
SELECT DISTINCT c.Id
FROM #customers c
LEFT JOIN #CustomersForCards ct ON ct.Id = c.Id
WHERE ct.Id IS NULL;
";
                await insertTempTableQuery.ExecuteNonQueryAsync();
            }

            var command = connection.CreateCommand();
            command.CommandText = @"
BEGIN TRANSACTION
    UPDATE AccumulationCards
    SET Status = 1, UpdatedAt = GETUTCDATE()
    WHERE Id = @CardId;
    
    INSERT INTO CustomerAccumulationCards(CustomerId, AccumulationCardId, Counter, Status, AssignedAt)
    SELECT Id, @CardId, 0, 0, GETUTCDATE()
    FROM #CustomersForCards
    ORDER BY NEWID();

COMMIT TRANSACTION;
";
            var cardIdParam = command.CreateParameter();
            cardIdParam.ParameterName = "@CardId";
            cardIdParam.Value = card.Id;
            command.Parameters.Add(cardIdParam);

            await command.ExecuteNonQueryAsync();
            await _logHistoryService.AddRangeAsync(LogHistoryHelper.GetPublishAccumulationCardLog(card.Id, _userContext.AdminId));
;

            if (card.Coupons.First().ExpirationDate <= DateTime.UtcNow.AddHours(JobConstants.LoyaltyExpirationJobIntervalHours))
            {
                await _jobSchedulerService.SetAccumulationExpirationJobAsync(card.Id, card.Coupons.First().ExpirationDate, true);
            }

            if (card.Coupons.First().StartDate < DateTime.UtcNow.AddMinutes(30))
            {
                await _jobSchedulerService.SetAccumulationCardActiveNotificationJobAsync(card.Id, card.Coupons.First().StartDate, true);
            }
        }

        public async Task CompleteAsync(int customerAccumulationCardId, int rewardCouponId)
        {
            var query = await GetCustomerQueryAsync();

            var item = await query
                .FirstOrDefaultAsync(x => x.CustomerAccumulationCard.Id == customerAccumulationCardId);

            if (item == null)
            {
                throw new ArgumentException("Картка не знайдена");
            }

            var rewardCoupon = item.CustomerAccumulationCard.AccumulationCard.Coupons.FirstOrDefault(x => x.Id == rewardCouponId);

            if (rewardCoupon == null)
            {
                throw new ArgumentException("Невалідна нагорода");
            }
            
            if (item.CustomerAccumulationCard.Counter == item.CustomerAccumulationCard.AccumulationCard.CountToComplete)
            {
                item.CustomerAccumulationCard.Status = CustomerAccumulationCardStatus.Completed;
                item.CustomerAccumulationCard.UsedAt = DateTime.UtcNow;
                await _integrationDataContext.CustomerCoupons.AddAsync(new CustomerCoupon
                {
                    CustomerId = item.CustomerAccumulationCard.CustomerId,
                    UsedTimes = 0,
                    CouponId = rewardCouponId,
                    Activated = true,
                });
                await _integrationDataContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Картка ще не заповнена");
            }
        }

        public async Task<CustomerAccumulationCardCountDTO> CountForCustomerAsync(int? storeId)
        {
            var result = new CustomerAccumulationCardCountDTO();
            var query = await GetCustomerQueryAsync();

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                query = query.Where(x => x.Coupon.StoreCodes.Any(x => x.StoreCode == storeCode));
            }

            result.TotalCount = await query.CountAsync();
            result.ActiveCount = await query.Where(x => x.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Active).CountAsync();

            return result;
        }

        public async Task SetApplicableCardsActiveFlagAsync(bool activeFlag, int? storeId = null)
        {
            var query = await GetCustomerQueryAsync();

            query = query.Where(x => x.CustomerAccumulationCard.Counter != x.CustomerAccumulationCard.AccumulationCard.CountToComplete);

            if (storeId.HasValue)
            {
                var storeCode = await _appUnitOfWork.Stores.Find(x => x.Id == storeId).Select(x => x.Number).FirstAsync();
                query = query.Where(x => x.Coupon.StoreCodes.Any(x => x.StoreCode == storeCode));
            }

            if (activeFlag)
            {
                await query.Where(x => x.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Inactive)
                    .Select(x => x.CustomerAccumulationCard)
                    .ForEachAsync(x => x.Status = CustomerAccumulationCardStatus.Active);
            }
            else
            {
                await query.Where(x => x.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Active)
                    .Select(x => x.CustomerAccumulationCard)
                    .ForEachAsync(x => x.Status = CustomerAccumulationCardStatus.Inactive);
            }

            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task<PagedListDTO<CustomerAccumulationCardForAdminDTO>> GetCustomerCardsByAdminAsync(int customerId, PagingDTO pagingDTO, RangeDTO<DateTime>? startDate = null, RangeDTO<DateTime>? expirationDate = null, CustomerAccumulationCardStatusForAdmin? status = null)
        {
            var query = _integrationDataContext.CustomerAccumulationCards
                .Include(x => x.AccumulationCard)
                    .ThenInclude(x => x.Coupons)
                        .ThenInclude(x => x.CouponProductFixedPrice)
                            .ThenInclude(x => x.Product)
                .Include(x => x.AccumulationCard)
                    .ThenInclude(x => x.Coupons)
                        .ThenInclude(x => x.StoreCodes)
                .Where(x => x.CustomerId == customerId)
                .Select(x => new CustomerAccumulationCardAndCouponItem { CustomerAccumulationCard = x, Coupon = x.AccumulationCard.Coupons.First() });

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
                if (status == CustomerAccumulationCardStatusForAdmin.Expired)
                {
                    query = query.Where(x => x.CustomerAccumulationCard.Status != CustomerAccumulationCardStatus.Completed && (x.Coupon.Status == CouponStatus.Archived && x.Coupon.UpdatedAt >= x.Coupon.ExpirationDate) && x.Coupon.ExpirationDate <= DateTime.UtcNow);
                }

                if (status == CustomerAccumulationCardStatusForAdmin.Inactive)
                {
                    query = query.Where(x => x.Coupon.Status == CouponStatus.Active
                        && x.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Inactive
                        && x.Coupon.Status == CouponStatus.Active
                        && x.Coupon.ExpirationDate <= DateTime.UtcNow);
                }

                if (status == CustomerAccumulationCardStatusForAdmin.Completed)
                {
                    query = query.Where(x => x.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Completed);
                }

                if (status == CustomerAccumulationCardStatusForAdmin.Active)
                {
                    query = query.Where(x => x.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Active
                        && x.Coupon.Status == CouponStatus.Active
                    );
                }

                if (status == CustomerAccumulationCardStatusForAdmin.Archived)
                {
                    query = query.Where(x => x.CustomerAccumulationCard.Status != CustomerAccumulationCardStatus.Completed 
                    && x.Coupon.UpdatedAt <= x.Coupon.ExpirationDate
                    && x.Coupon.Status == CouponStatus.Archived);
                }
            }

            query = query.OrderBy(x => x.CustomerAccumulationCard.Id);

            return await query.GetPagedListAsync(pagingDTO, ToCustomerAccumulationCardForAdminDTOAsync);
        }

        private async Task<CustomerAccumulationCardForAdminDTO> ToCustomerAccumulationCardForAdminDTOAsync(CustomerAccumulationCardAndCouponItem item)
        {
            // Fetch store IDs from the database based on StoreCodes
            var storeIds = await _appUnitOfWork.Stores.GetAllStores()
                .Where(store => item.Coupon.StoreCodes.Select(sc => sc.StoreCode).Contains(store.Number))
                .Select(store => store.Id)
                .ToListAsync();

            return new CustomerAccumulationCardForAdminDTO
            {
                Status = GetCustomerAdminStatus(item),
                Counter = item.CustomerAccumulationCard.Counter,
                CountToComplete = item.CustomerAccumulationCard.AccumulationCard.CountToComplete,
                Description = item.CustomerAccumulationCard.AccumulationCard.Description,
                CouponDescription = item.Coupon.Description,
                Icon = item.CustomerAccumulationCard.AccumulationCard.Icon,
                CustomerAccumulationCardId = item.CustomerAccumulationCard.Id,
                Id = item.CustomerAccumulationCard.AccumulationCard.Id,
                Image = item.CustomerAccumulationCard.AccumulationCard.Coupons.First().Image,
                Name = item.CustomerAccumulationCard.AccumulationCard.Name,
                StartDate = item.CustomerAccumulationCard.AccumulationCard.Coupons.First().StartDate,
                ExpirationDate = item.CustomerAccumulationCard.AccumulationCard.Coupons.First().ExpirationDate,
                ActivationProducts = item.CustomerAccumulationCard.AccumulationCard.Coupons.Select(x => new ProductShortInfoDTO
                {
                    Code = x.CouponProductFixedPrice.Product.Code,
                    Title = x.CouponProductFixedPrice.Product.Title,
                    Image = x.CouponProductFixedPrice.Product.ImagePath,
                }).ToList(),
                Rewards = item.CustomerAccumulationCard.AccumulationCard.Coupons.Select(x => new AccumulationCardReward
                {
                    CouponId = x.Id,
                    Name = x.Name,
                    Price = x.CouponProductFixedPrice.Price,
                    Product = new ProductShortInfoDTO
                    {
                        Code = x.CouponProductFixedPrice.Product.Code,
                        Title = x.CouponProductFixedPrice.Product.Title,
                        Image = x.CouponProductFixedPrice.Product.ImagePath,
                    },
                }).ToList(),
                StoreIds = storeIds,
            };
        }

        private static CustomerAccumulationCardStatusForAdmin GetCustomerAdminStatus(CustomerAccumulationCardAndCouponItem item)
        {
            if (item.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Completed)
            {
                return CustomerAccumulationCardStatusForAdmin.Completed;
            }

            if (item.Coupon.Status == CouponStatus.Archived)
            {
                return item.Coupon.UpdatedAt <= item.Coupon.ExpirationDate
                    ? CustomerAccumulationCardStatusForAdmin.Archived
                    : CustomerAccumulationCardStatusForAdmin.Expired;
            }

            return item.CustomerAccumulationCard.Status == CustomerAccumulationCardStatus.Active
                ? CustomerAccumulationCardStatusForAdmin.Active
                : CustomerAccumulationCardStatusForAdmin.Inactive;
        }
    }

    public class CustomerAccumulationCardAndCouponItem
    {
        public CustomerAccumulationCard CustomerAccumulationCard { get; set; }
        public Coupon Coupon { get; set; }
    }

    public class AccumulationCardAndCouponItem
    {
        public AccumulationCard AccumulationCard { get; set; }
        public Coupon Coupon { get; set; }
    }
}
