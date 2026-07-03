using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Targets;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace JetFlight.Service.Services
{
    public interface ITargetService
    {
        Task<TargetDto> CreateAsync(BaseTargetDto model);
        Task<PagedListDTO<TargetDto>> GetAllAsync(PagingDTO pagingDTO, byte? branchId);
        Task<TargetDto> GetAsync(int id);
        Task UpdateAsync(TargetDto model);
        Task DeleteAsync(int id);
        Task<Dictionary<Branches, int>> CalculateAudienceAsync(int targetId);
        Task PopulateTargetCustomersIntoTempTableAsync(Target target, DbConnection connection);
        Task<Target> GetTargetEntityAsync(int id);
    }

    public class TargetService : ITargetService
    {
        private readonly IDataUnitOfWork _unitOfWork;
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly IDataUnitOfWork _appUnitOfWork;

        public TargetService(IDataUnitOfWork unitOfWork, IntegrationDataContext integrationDataContext, IDataUnitOfWork appUnitOfWork)
        {
            _unitOfWork = unitOfWork;
            _integrationDataContext = integrationDataContext;
            _appUnitOfWork = appUnitOfWork;
        }

        public async Task<TargetDto> GetAsync(int id)
        {
            var target = await GetQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (target == null)
            {
                throw new ArgumentException("Таргет не знайден");
            }

            var result = ToDto(target);

            return result;
        }

        public async Task<PagedListDTO<TargetDto>> GetAllAsync(PagingDTO pagingDTO, byte? branchId)
        {
            var query = GetQuery()
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query.Where(x => !x.BranchId.HasValue || x.BranchId == branchId);
            }

            var targets = await query.GetPagedListAsync(pagingDTO, ToDto);
            return targets;
        }

        public async Task UpdateAsync(TargetDto model)
        {
            var target = await GetQuery().FirstOrDefaultAsync(x => x.Id == model.Id);

            if (target == null)
            {
                throw new ArgumentException("Таргет не знайден");
            }

            await SetTargetAsync(model, target);
            target.UpdatedAt = DateTime.UtcNow.SetKindUtc();

            await _unitOfWork.Save(true);
        }

        public async Task<TargetDto> CreateAsync(BaseTargetDto model)
        {
            var entity = await SetTargetAsync(model);
            entity.CreatedAt = DateTime.UtcNow.SetKindUtc();
            await _unitOfWork.Targets.Add(entity);
            await _unitOfWork.Save(true);

            return ToDto(entity);
        }

        private async Task<Target> SetTargetAsync(BaseTargetDto model, Target? entity = null)
        {
            entity ??= new Target();

            entity.Name = model.Name;
            entity.BranchId = model.BranchId;

            model.CityIds ??= new();

            foreach (var cityId in model.CityIds)
            {
                if (!await _appUnitOfWork.Cities.Any(x => x.Id == cityId))
                {
                    throw new ArgumentException($"Помилка з id міста {cityId}");
                }
            }

            entity.Cities ??= new();
            entity.Cities.RemoveAll(x => !model.CityIds.Contains(x.Id));

            var citiesToAdd = model.CityIds.Except(entity.Cities.Select(x => x.Id)).ToList();
            foreach (var cityId in citiesToAdd)
            {
                entity.Cities.Add(new TargetToCity
                {
                    CityId = cityId
                });
            }

            // Age filters

            entity.AgeFrom = model.Age?.From;
            entity.AgeTo = model.Age?.To;
            entity.DayOfBirthFrom = model.DayOfBirth?.From;
            entity.DayOfBirthTo = model.DayOfBirth?.To;
            entity.MonthOfBirthFrom = model.MonthOfBirth?.From;
            entity.MonthOfBirthTo = model.MonthOfBirth?.To;
            entity.YearOfBirthFrom = model.YearOfBirth?.From;
            entity.YearOfBirthTo = model.YearOfBirth?.To;

            entity.Sex = model.Sex;

            // Shopping filters

            entity.ShoppingPeriodFrom = model.ShoppingPeriod?.From;
            entity.ShoppingPeriodTo = model.ShoppingPeriod?.To;
            entity.AverageCheckPositionsFrom = model.AverageCheckPositions?.From;
            entity.AverageCheckPositionsTo = model.AverageCheckPositions?.To;
            entity.AverageCheckAmountFrom = model.AverageCheckAmount?.From;
            entity.AverageCheckAmountTo = model.AverageCheckAmount?.To;
            entity.ShoppingTimeFrom = model.ShoppingTime?.From;
            entity.ShoppingTimeTo = model.ShoppingTime?.To;
            entity.FrequencyType = model.FrequencyShopping?.FrequencyType;
            entity.AverageFrequencyTimes = model.FrequencyShopping?.AverageTimes;
            entity.RegisteredDateFrom = model.RegisteredDate?.From;
            entity.RegisteredDateTo = model.RegisteredDate?.To;

            // Period filters

            entity.Period = model.Period;
            entity.CheckCountFrom = model.CheckCount?.From;
            entity.CheckCountTo = model.CheckCount?.To;
            entity.IncludeOnBirthdayOnly = model.IncludeOnBirthdayOnly;
            entity.RegisteredDaysFrom = model.RegisteredDays?.From;
            entity.RegisteredDaysTo = model.RegisteredDays?.To;

            // Product filters

            entity.ProductAmountTo = model.ProductAmount?.To;
            entity.ProductAmountFrom = model.ProductAmount?.From;
            if (model.ManufacturerCodes?.Any() == true)
            {
                entity.ManufacturerCodes = await _integrationDataContext
                    .ProductManufacturers
                    .Where(x => model.ManufacturerCodes.Contains(x.Code))
                    .Select(x => x.Code).ToListAsync();
            }
            else
            {
                entity.ManufacturerCodes = null;
            }

            if (model.CategoryCodes?.Any() == true)
            {
                entity.CategoryCodes = await _integrationDataContext
                    .ProductCategories
                    .Where(x => model.CategoryCodes.Contains(x.Code))
                    .Select(x => x.Code).ToListAsync();
            }
            else
            {
                entity.CategoryCodes = null;
            }


            model.RFMIds ??= new();
            entity.RFMs ??= new();
            entity.RFMs.RemoveAll(x => !model.RFMIds.Contains(x.Id));
            var rfmsToAdd = model.RFMIds.Except(entity.RFMs.Select(x => x.Id)).ToList();
            foreach (var rfmId in rfmsToAdd)
            {
                entity.RFMs.Add(new TargetToRFM
                {
                    RFMId = rfmId
                });
            }

            return entity;
        }

        private static TargetDto ToDto(Target x)
            => new TargetDto
            {
                Age = new RangeDTO<int?> { From = x.AgeFrom, To = x.AgeTo },
                Sex = x.Sex,
                AverageCheckAmount = new RangeDTO<decimal?> { From = x.AverageCheckAmountFrom, To = x.AverageCheckAmountTo },
                AverageCheckPositions = new RangeDTO<int?> { From = x.AverageCheckPositionsFrom, To = x.AverageCheckPositionsTo },
                CategoryCodes = x.CategoryCodes,
                CityIds = x.Cities.Select(x => x.CityId).ToList(),
                ManufacturerCodes = x.ManufacturerCodes,
                DayOfBirth = new RangeDTO<int?> { From = x.DayOfBirthFrom, To = x.DayOfBirthTo },
                IncludeOnBirthdayOnly = x.IncludeOnBirthdayOnly,
                MonthOfBirth = new RangeDTO<Month?> { From = x.MonthOfBirthFrom, To = x.MonthOfBirthTo },
                RegisteredDate = new RangeDTO<DateTime?> { From = x.RegisteredDateFrom, To = x.RegisteredDateTo },
                RegisteredDays = new RangeDTO<int?> { From = x.RegisteredDaysFrom, To = x.RegisteredDaysTo },
                RFMIds = x.RFMs.Select(x => x.RFMId).ToList(),
                ShoppingPeriod = new RangeDTO<Month?> { From = x.ShoppingPeriodFrom, To = x.ShoppingPeriodTo },
                ShoppingTime = new RangeDTO<TimeSpan?> { From = x.ShoppingTimeFrom, To = x.ShoppingTimeTo },
                YearOfBirth = new RangeDTO<int?> { From = x.YearOfBirthFrom, To = x.YearOfBirthTo },
                CheckCount = new RangeDTO<int?> { From = x.CheckCountFrom, To = x.CheckCountTo }, 
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Period = x.Period,
                ProductAmount = new RangeDTO<decimal?> { From = x.ProductAmountFrom, To = x.ProductAmountTo },
                FrequencyShopping = x.FrequencyType.HasValue && x.AverageFrequencyTimes.HasValue
                    ? new FrequencyShoppingDTO
                    {
                        AverageTimes = x.AverageFrequencyTimes.Value,
                        FrequencyType = x.FrequencyType.Value
                    }
                    : null,
                Name = x.Name,
                BranchId = x.BranchId,
                Id = x.Id,
            };

        private IQueryable<Target> GetQuery()
            => _unitOfWork.Targets.GetAll()
                .Include(x => x.Cities)
                // RFM
                .Include(x => x.RFMs);

        public async Task DeleteAsync(int id)
        {
            var target = await _unitOfWork.Targets.GetAll().FirstOrDefaultAsync(x => x.Id == id);

            if (target == null)
            {
                throw new ArgumentException("Таргет не знайден");
            }

            _unitOfWork.Targets.Remove(target);
            await _unitOfWork.Save(true);
        }

        /// <summary>
        /// Result are stored in #customers
        /// </summary>
        /// <param name="target"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task PopulateTargetCustomersIntoTempTableAsync(Target target, DbConnection connection)
        {
            var createResultTableCommand = connection.CreateCommand();
            createResultTableCommand.CommandText = @"
DROP TABLE IF EXISTS #customers;

CREATE TABLE #customers
(
Id integer NOT NULL,
BranchId TINYINT NOT NULL,
);
";
            await createResultTableCommand.ExecuteNonQueryAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
EXEC USP_TargetCalculation 
@BranchId,
@StoreCodes,
@AgeFrom, @AgeTo,
@DayOfBirthFrom, @DayOfBirthTo,
@MonthOfBirthFrom, @MonthOfBirthTo,
@YearOfBirthFrom, @YearOfBirthTo,
@Sex,
@ShoppingPeriodFrom, @ShoppingPeriodTo,
@AverageCheckPositionsFrom, @AverageCheckPositionsTo,
@AverageCheckAmountFrom, @AverageCheckAmountTo,
@ShoppingTimeFrom, @ShoppingTimeTo,
@RegisteredDateFrom, @RegisteredDateTo,
@FrequencyType, @AverageFrequencyTimes,
@Period,
@CheckCountFrom, @CheckCountTo,
@IncludeOnBirthdayOnly,
@RegisteredDaysFrom, @RegisteredDaysTo,
@ProductAmountFrom, @ProductAmountTo,
@ManufacturerCodes,
@CategoryCodes,
@RFMs
";

            var branchIdParam = new SqlParameter();
            branchIdParam.ParameterName = "@BranchId";
            branchIdParam.Value = target.BranchId ?? (object)DBNull.Value;
            command.Parameters.Add(branchIdParam);

            var cityIds = target.Cities.Select(x => x.CityId).ToList();
            var stores = await _unitOfWork.Stores.Find(x => cityIds.Contains(x.CityId) && (!target.BranchId.HasValue || target.BranchId == x.BranchId))
                .Select(x => x.Id.ToString())
                .ToListAsync();

            var storesParam = new SqlParameter();
            storesParam.ParameterName = "@StoreCodes";
            storesParam.Value = AsEnumerableParameter(stores);
            storesParam.SqlDbType = SqlDbType.Structured;
            storesParam.TypeName = "StringList";
            command.Parameters.Add(storesParam);

            var ageFromParam = command.CreateParameter();
            ageFromParam.ParameterName = "@AgeFrom";
            ageFromParam.Value = target.AgeFrom ?? (object)DBNull.Value;
            command.Parameters.Add(ageFromParam);

            var ageToParam = command.CreateParameter();
            ageToParam.ParameterName = "@AgeTo";
            ageToParam.Value = target.AgeTo ?? (object)DBNull.Value;
            command.Parameters.Add(ageToParam);

            var dayOfBirthFromParam = command.CreateParameter();
            dayOfBirthFromParam.ParameterName = "@DayOfBirthFrom";
            dayOfBirthFromParam.Value = target.DayOfBirthFrom ?? (object)DBNull.Value;
            command.Parameters.Add(dayOfBirthFromParam);

            var dayOfBirthToParam = command.CreateParameter();
            dayOfBirthToParam.ParameterName = "@DayOfBirthTo";
            dayOfBirthToParam.Value = target.DayOfBirthTo ?? (object)DBNull.Value;
            command.Parameters.Add(dayOfBirthToParam);

            var monthOfBirthFromParam = command.CreateParameter();
            monthOfBirthFromParam.ParameterName = "@MonthOfBirthFrom";
            monthOfBirthFromParam.Value = target.MonthOfBirthFrom ?? (object)DBNull.Value;
            command.Parameters.Add(monthOfBirthFromParam);

            var monthOfBirthToParam = command.CreateParameter();
            monthOfBirthToParam.ParameterName = "@MonthOfBirthTo";
            monthOfBirthToParam.Value = target.MonthOfBirthTo ?? (object)DBNull.Value;
            command.Parameters.Add(monthOfBirthToParam);

            var yearOfBirthFromParam = command.CreateParameter();
            yearOfBirthFromParam.ParameterName = "@YearOfBirthFrom";
            yearOfBirthFromParam.Value = target.YearOfBirthFrom ?? (object)DBNull.Value;
            command.Parameters.Add(yearOfBirthFromParam);

            var yearOfBirthToParam = command.CreateParameter();
            yearOfBirthToParam.ParameterName = "@YearOfBirthTo";
            yearOfBirthToParam.Value = target.YearOfBirthTo ?? (object)DBNull.Value;
            command.Parameters.Add(yearOfBirthToParam);

            var sexParam = command.CreateParameter();
            sexParam.ParameterName = "@Sex";
            sexParam.Value = target.Sex ?? (object)DBNull.Value;
            command.Parameters.Add(sexParam);

            var shoppingPeriodFromParam = command.CreateParameter();
            shoppingPeriodFromParam.ParameterName = "@ShoppingPeriodFrom";
            shoppingPeriodFromParam.Value = target.ShoppingPeriodFrom ?? (object)DBNull.Value;
            command.Parameters.Add(shoppingPeriodFromParam);

            var shoppingPeriodToParam = command.CreateParameter();
            shoppingPeriodToParam.ParameterName = "@ShoppingPeriodTo";
            shoppingPeriodToParam.Value = target.ShoppingPeriodTo ?? (object)DBNull.Value;
            command.Parameters.Add(shoppingPeriodToParam);

            var averageCheckPositionsFromParam = command.CreateParameter();
            averageCheckPositionsFromParam.ParameterName = "@AverageCheckPositionsFrom";
            averageCheckPositionsFromParam.Value = target.AverageCheckPositionsFrom ?? (object)DBNull.Value;
            command.Parameters.Add(averageCheckPositionsFromParam);

            var averageCheckPositionsToParam = command.CreateParameter();
            averageCheckPositionsToParam.ParameterName = "@AverageCheckPositionsTo";
            averageCheckPositionsToParam.Value = target.AverageCheckPositionsTo ?? (object)DBNull.Value;
            command.Parameters.Add(averageCheckPositionsToParam);

            var averageCheckAmountFromParam = command.CreateParameter();
            averageCheckAmountFromParam.ParameterName = "@AverageCheckAmountFrom";
            averageCheckAmountFromParam.Value = target.AverageCheckAmountFrom ?? (object)DBNull.Value;
            command.Parameters.Add(averageCheckAmountFromParam);

            var averageCheckAmountToParam = command.CreateParameter();
            averageCheckAmountToParam.ParameterName = "@AverageCheckAmountTo";
            averageCheckAmountToParam.Value = target.AverageCheckAmountTo ?? (object)DBNull.Value;
            command.Parameters.Add(averageCheckAmountToParam);

            var shoppingTimeFromParam = command.CreateParameter();
            shoppingTimeFromParam.ParameterName = "@ShoppingTimeFrom";
            shoppingTimeFromParam.Value = target.ShoppingTimeFrom ?? (object)DBNull.Value;
            command.Parameters.Add(shoppingTimeFromParam);

            var shoppingTimeToParam = command.CreateParameter();
            shoppingTimeToParam.ParameterName = "@ShoppingTimeTo";
            shoppingTimeToParam.Value = target.ShoppingTimeTo ?? (object)DBNull.Value;
            command.Parameters.Add(shoppingTimeToParam);

            var registeredDateFromParam = command.CreateParameter();
            registeredDateFromParam.ParameterName = "@RegisteredDateFrom";
            registeredDateFromParam.DbType = DbType.DateTime2;
            registeredDateFromParam.Value = target.RegisteredDateFrom ?? (object)DBNull.Value;
            command.Parameters.Add(registeredDateFromParam);

            var registeredDateToParam = command.CreateParameter();
            registeredDateToParam.ParameterName = "@RegisteredDateTo";
            registeredDateToParam.DbType = DbType.DateTime2;
            registeredDateToParam.Value = target.RegisteredDateTo ?? (object)DBNull.Value;
            command.Parameters.Add(registeredDateToParam);

            var frequencyTypeParam = command.CreateParameter();
            frequencyTypeParam.ParameterName = "@FrequencyType";
            frequencyTypeParam.Value = target.FrequencyType ?? (object)DBNull.Value;
            command.Parameters.Add(frequencyTypeParam);

            var averageFrequencyTimesParam = command.CreateParameter();
            averageFrequencyTimesParam.ParameterName = "@AverageFrequencyTimes";
            averageFrequencyTimesParam.Value = target.AverageFrequencyTimes ?? (object)DBNull.Value;
            command.Parameters.Add(averageFrequencyTimesParam);

            var periodParam = command.CreateParameter();
            periodParam.ParameterName = "@Period";
            periodParam.Value = target.Period ?? (object)DBNull.Value;
            command.Parameters.Add(periodParam);

            var checkCountFrom = command.CreateParameter();
            checkCountFrom.ParameterName = "@CheckCountFrom";
            checkCountFrom.Value = target.CheckCountFrom ?? (object)DBNull.Value;
            command.Parameters.Add(checkCountFrom);

            var checkCountToParam = command.CreateParameter();
            checkCountToParam.ParameterName = "@CheckCountTo";
            checkCountToParam.Value = target.CheckCountTo ?? (object)DBNull.Value;
            command.Parameters.Add(checkCountToParam);

            var includeOnBirthdayOnlyParam = command.CreateParameter();
            includeOnBirthdayOnlyParam.ParameterName = "@IncludeOnBirthdayOnly";
            includeOnBirthdayOnlyParam.Value = target.IncludeOnBirthdayOnly;
            command.Parameters.Add(includeOnBirthdayOnlyParam);

            var registeredDaysFromParam = command.CreateParameter();
            registeredDaysFromParam.ParameterName = "@RegisteredDaysFrom";
            registeredDaysFromParam.Value = target.RegisteredDaysFrom ?? (object)DBNull.Value;
            command.Parameters.Add(registeredDaysFromParam);

            var registeredDaysToParam = command.CreateParameter();
            registeredDaysToParam.ParameterName = "@RegisteredDaysTo";
            registeredDaysToParam.Value = target.RegisteredDaysTo ?? (object)DBNull.Value;
            command.Parameters.Add(registeredDaysToParam);


            var productAmountFromParam = command.CreateParameter();
            productAmountFromParam.ParameterName = "@ProductAmountFrom";
            productAmountFromParam.Value = target.ProductAmountFrom ?? (object)DBNull.Value;
            command.Parameters.Add(productAmountFromParam);

            var productAmountToParam = command.CreateParameter();
            productAmountToParam.ParameterName = "@ProductAmountTo";
            productAmountToParam.Value = target.ProductAmountTo ?? (object)DBNull.Value;
            command.Parameters.Add(productAmountToParam);

            var manufacturerCodesParam = new SqlParameter();
            manufacturerCodesParam.ParameterName = "@ManufacturerCodes";
            manufacturerCodesParam.Value = AsEnumerableParameter(target.ManufacturerCodes);
            manufacturerCodesParam.SqlDbType = SqlDbType.Structured;
            manufacturerCodesParam.TypeName = "StringList";
            command.Parameters.Add(manufacturerCodesParam);

            var categoryCodesParam = new SqlParameter();
            categoryCodesParam.ParameterName = "@CategoryCodes";
            categoryCodesParam.Value = AsEnumerableParameter(target.CategoryCodes);
            categoryCodesParam.SqlDbType = SqlDbType.Structured;
            categoryCodesParam.TypeName = "StringList";
            command.Parameters.Add(categoryCodesParam);

            var rfmParam = new SqlParameter();
            rfmParam.ParameterName = "@RFMs";
            rfmParam.Value = await GetRfmParamererAsync(target.RFMs?.Select(x => x.RFMId).ToList());
            rfmParam.SqlDbType = SqlDbType.Structured;
            rfmParam.TypeName = "RFMList";
            command.Parameters.Add(rfmParam);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<Dictionary<Branches,int>> CalculateAudienceAsync(int targetId)
        {
            var target = await GetQuery().FirstOrDefaultAsync(x => x.Id == targetId);

            if (target == null)
            {
                throw new ArgumentException("Таргет не знайден");
            }

            using var connection = _integrationDataContext.Database.GetDbConnection();
            connection.Open();

            await PopulateTargetCustomersIntoTempTableAsync(target, connection);

            var countCommand = connection.CreateCommand();
            countCommand.CommandText = @"
SELECT BranchId, COUNT(1) AS Count FROM #customers
GROUP BY BranchId;
";

            

            var result = new Dictionary<Branches, int>();

            if (target.BranchId.HasValue)
            {
                result[(Branches)target.BranchId.Value] = 0; 
            }
            else
            {
                foreach (var branch in Enum.GetValues<Branches>())
                {
                    result[branch] = 0;
                }
            }
            
            using var reader = await countCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var resulBranchId = reader.GetByte(0);
                var resultCount = reader.GetInt32(1);

                result[(Branches)resulBranchId] = resultCount;
            }

            return result;
        }
        
        public async Task<Target> GetTargetEntityAsync(int id)
        {
            return await GetQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        private async Task<DataTable> GetRfmParamererAsync(List<int> rfmIds)
        {
            var datatable = new DataTable();

            datatable.Columns.Add("PeriodFrom");
            datatable.Columns.Add("PeriodTo");
            datatable.Columns.Add("AmountFrom");
            datatable.Columns.Add("AmountTo");
            datatable.Columns.Add("CountFrom");
            datatable.Columns.Add("CountTo");

            if (rfmIds == null)
            {
                return datatable;
            }

            foreach (var rfmId in rfmIds)
            {
                var rfm = await _unitOfWork.RFMs
                    .GetAll()
                    .Include(x => x.Period)
                    .Include(x => x.Amount)
                    .Include(x => x.Count)
                    .AsNoTracking()
                    .FirstAsync(x => x.Id == rfmId);

                datatable.Rows.Add(rfm.Period.From, rfm.Period.To, rfm.Amount.From, rfm.Amount.To, rfm.Count.From, rfm.Count.To);
            }

            return datatable;
        }

        private DataTable AsEnumerableParameter(IEnumerable<string> elements)
        {
            var datatable = new DataTable();

            datatable.Columns.Add("Element");

            if (elements == null)
            {
                return datatable;
            }

            foreach (var elem in elements)
            {
                datatable.Rows.Add(elem);
            }

            return datatable;
        }
    }
}
