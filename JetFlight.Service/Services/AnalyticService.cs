using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Analytic;
using JetFlight.Shared.Models.Feedback;
using JetFlight.Shared.Models.Mouseflow;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Targets;
using JetFlight.Shared.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JetFlight.Service.Services
{
    public interface IAnalyticService
    {
        Task<CountAnalyticDto> GetActiveCustomerAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex, Granularity granularity);
        Task<AgeAnalyticDto> GetAgeAnalyticAsync(byte? branchId, int? cityId, int? activeStoreCode, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex);
        Task<BirthdayAnalyticDto> GetBirthdayAnalyticAsync(byte? branchId, int? cityId, int? activeStoreId, RangeDTO<Month> month, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex, Granularity granularity);
        Task<CustomerGeneralAnalyticsDto> GetGeneralAnalyticsAsync(byte? branchId, int? cityId, int? activeStoreCode, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex);
        Task<CountAnalyticDto> GetRegistrationAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex, Granularity granularity);
        Task<HourCountAnalyticDto> GetRegistrationHourAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex);
        Task<List<WhereFindOutAnalyticMetric>> GetWhereFindOutAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date, RangeDTO<int?> age, Sex? sex);
        Task<List<DislocationMetricDto>> GetDislocationAnalyticAsync(byte? branchId, int? cityId, RangeDTO<int?> age, Sex? sex);
        Task<ReceiptAnalyticDto> GetReceiptAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<PagedListDTO<ProductCategoryMetricDto>> GetProductCategoryAnalyticAsync(PagingDTO pagingDTO, byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<HourCountAnalyticDto> GetPurchaseTimeAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<GeneralCouponAnalyticDto> GetGeneralCouponAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<PagedListDTO<CouponMetricDto>> GetCouponAnalyticAsync(PagingDTO pagingDTO, byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<FeedbackAnalyticDto> GetFeedbackAnalytic(byte? branchId, FeedbackType? type, int? storeId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex);
        Task<List<TopicAnalyticMetric>> GetTopicAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date);
        Task<GeneralAccumulationCardAnalytic> GetGeneralAccumulationCardAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<PagedListDTO<AccumulationCardMetricDto>> GetAccumulationCardAnalyticAsync(PagingDTO pagingDTO, byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<ProgramUsageAnalytic> GetProgramUsageAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<List<PageViewMetricDto>> GetPageVisitAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date);
        Task<CountAnalyticDto> GetTradeTurnoverAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex, Granularity granularity);
        Task<CustomerCountAnalyticDto> GetCustomerCountAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<GeneralRegistrationAnalyticDto> GetGeneralRegistrationAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform);
        Task<GeneralRegistrationAnalyticDebugDto> GetGeneralRegistrationAnalyticDebugAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform);
        Task<CardAndCouponUsageAnalyticByCategoryDTO> GetCardAndCouponUsageAnalyticByCategoryAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<CouponTimeUsageAnalyticDto> GetCouponTimeUsageAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex);
        Task<List<TypeOfActivityAnalyticMetric>> GetTypeOfActivityAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date, RangeDTO<int?> age, Sex? sex);
        Task<List<NumberOfChildrenAnalyticMetric>> GetNumberOfChildrenAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date, RangeDTO<int?> age, Sex? sex);
        Task<ProgramSpentAnalyticDto> GetProgramSpentAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex, Granularity granularity);
        Task<RfmCustomerCountAnalyticDto> GetRfmCustomerCountAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex, Granularity granularity);
    }

    public class AnalyticService : IAnalyticService
    {
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly ApplicationDataContext _applicationDataContext;
        private readonly IMouseflowApi _mouseflowApi;
        private readonly MouseflowSettings _mouseflowSettings;
        private readonly AppSettings _appSettings;

        public AnalyticService(
            IntegrationDataContext integrationDataContext,
            ApplicationDataContext applicationDataContext,
            IMouseflowApi mouseflowApi,
            IOptions<MouseflowSettings> mouseflowSettings,
            IOptions<AppSettings> appSettings)
        {
            _integrationDataContext = integrationDataContext;
            _applicationDataContext = applicationDataContext;
            _mouseflowApi = mouseflowApi;
            _mouseflowSettings = mouseflowSettings.Value;
            _appSettings = appSettings.Value;
        }

        private IQueryable<Customer> GetBaseCustomerQuery(byte? branchId, int? activeStoreCode, RangeDTO<int?> age, Sex? sex, bool includeDeleted = false)
        {
            var query = _integrationDataContext.Customers.AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(x => !x.IsDeleted);
            }


            if (branchId.HasValue && activeStoreCode.HasValue)
            {
                query = query.Where(x => x.CustomerSettings.Any(x =>
                    x.BranchId == branchId && x.ActiveStoreId == activeStoreCode));
            }
            else if (branchId.HasValue)
            {
                query = query.Where(x => x.CustomerSettings.Any(x => x.BranchId == branchId));
            }
            else if (activeStoreCode.HasValue)
            {
                query = query.Where(x => x.CustomerSettings.Any(x => x.ActiveStoreId == activeStoreCode));
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => EF.Functions.DateDiffYear(x.Birthday, DateTime.UtcNow) <= age.To.Value);
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => EF.Functions.DateDiffYear(x.Birthday, DateTime.UtcNow) >= age.From.Value);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.Sex == sex);
            }

            return query;
        }

        public async Task<CustomerGeneralAnalyticsDto> GetGeneralAnalyticsAsync(byte? branchId, int? cityId, int? activeStoreCode, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex)
        {
            var generalAnalitics = new CustomerGeneralAnalyticsDto();

            var query = GetBaseCustomerQuery(branchId, activeStoreCode, age, sex, includeDeleted: true);

            if (clientPlatform.HasValue)
            {
                var registrationPlatfrom = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                query = query.Where(x => x.RegistrationPlatform == registrationPlatfrom);
            }

            if (cityId.HasValue)
            {
                var storeIds = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Id).ToListAsync();

                query = query.Where(x => x.CustomerSettings.Any(x => (!branchId.HasValue || x.BranchId == branchId) && x.ActiveStoreId.HasValue && storeIds.Contains(x.ActiveStoreId!.Value)));
            }

            generalAnalitics.Count = await query.CountAsync();
            generalAnalitics.MaleCount = await query.Where(x => x.Sex == Sex.Male).CountAsync();
            generalAnalitics.FemaleCount = await query.Where(x => x.Sex == Sex.Female).CountAsync();

            return generalAnalitics;
        }

        public async Task<AgeAnalyticDto> GetAgeAnalyticAsync(byte? branchId, int? cityId, int? activeStoreId, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex)
        {
            var query = GetBaseCustomerQuery(branchId, activeStoreId, age, sex);

            if (clientPlatform.HasValue)
            {
                var registrationPlatfrom = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                query = query.Where(x => x.RegistrationPlatform == registrationPlatfrom);
            }

            if (cityId.HasValue)
            {
                var storeIds = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Id).ToListAsync();

                query = query.Where(x => x.CustomerSettings.Any(x => (!branchId.HasValue || x.BranchId == branchId) && x.ActiveStoreId.HasValue && storeIds.Contains(x.ActiveStoreId!.Value)));
            }

            var metrics = await query
                .Where(x => x.Birthday.HasValue)
                .Select(x => EF.Functions.DateDiffYear(x.Birthday!.Value, DateTime.UtcNow))
                .GroupBy(x => x)
                .Select(x => new AgeMetricDto
                {
                    Age = x.Key,
                    Count = x.Count()
                })
                .OrderBy(x => x.Age)
                .ToListAsync();

            var analytic = new AgeAnalyticDto
            {
                Metrics = metrics,
            };

            if (metrics.Count != 0)
            {
                analytic.MaxAge = metrics.Last().Age;
                analytic.MinAge = metrics.First().Age;
                analytic.AverageAge = metrics.Sum(x => x.Age * x.Count) / metrics.Sum(x => x.Count);
            }

            return analytic;
        }

        public async Task<BirthdayAnalyticDto> GetBirthdayAnalyticAsync(byte? branchId, int? cityId, int? activeStoreId, RangeDTO<Month> month, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex, Granularity granularity)
        {
            if (month.From > month.To)
            {
                throw new ArgumentException("Month To should be greater or equal to Month from");
            }

            var query = GetBaseCustomerQuery(branchId, activeStoreId, age, sex);

            if (cityId.HasValue)
            {
                var storeIds = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Id).ToListAsync();

                query = query.Where(x => x.CustomerSettings.Any(x => (!branchId.HasValue || x.BranchId == branchId) && x.ActiveStoreId.HasValue && storeIds.Contains(x.ActiveStoreId!.Value)));
            }

            if (clientPlatform.HasValue)
            {
                var registrationPlatfrom = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                query = query.Where(x => x.RegistrationPlatform == registrationPlatfrom);
            }

            query = query.Where(x => x.Birthday.HasValue && x.Birthday.Value.Month >= (int)month.From && x.Birthday!.Value.Month <= (int)month.To);

            var metrics = await query
                .GroupBy(x => new { x.Birthday!.Value.Month, Day = granularity == Granularity.Day ? x.Birthday!.Value.Day : (int?)null })
                .Select(x => new BirthdayMetricDto
                {
                    Count = x.Count(),
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day,
                }).ToListAsync();

            var missingMetrics = new List<BirthdayMetricDto>();

            foreach (var (monthKey, dayKey) in GetBirthdayAnalyticKeys(month.From, month.To, granularity))
            {
                if (!metrics.Exists(m => m.Month == monthKey && m.Day == dayKey))
                {
                    missingMetrics.Add(new BirthdayMetricDto
                    {
                        Month = monthKey,
                        Day = dayKey,
                        Count = 0,
                    });
                }
            }

            metrics.AddRange(missingMetrics);

            metrics = metrics.OrderBy(x => x.Month).ThenBy(x => x.Day).ToList();

            var result = new BirthdayAnalyticDto
            {
                Metrics = metrics,
                Max = metrics.MaxBy(x => x.Count)!,
                Min = metrics.MinBy(x => x.Count)!,
            };

            return result;
        }

        private static IEnumerable<(Month Month, int? Day)> GetBirthdayAnalyticKeys(Month from, Month to, Granularity granularity)
        {
            for (var i = from; i <= to; i++)
            {
                if (granularity == Granularity.Month)
                {
                    yield return (i, null);
                }
                else
                {
                    var days = GetDaysInMonth(i);
                    foreach (var day in days)
                    {
                        yield return (i, day);
                    }
                }
            }
        }

        public async Task<CountAnalyticDto> GetActiveCustomerAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex, Granularity granularity)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (clientPlatform.HasValue)
            {
                var registrationPlatfrom = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                query = query.Where(x => x.CustomerCard.Customer.RegistrationPlatform == registrationPlatfrom);
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var metrics = await query
                .GroupBy(x => new { x.CreatedAt.Month, x.CreatedAt.Year, Day = granularity == Granularity.Day ? x.CreatedAt.Day : (int?)null })
                .Select(x => new CountMetricDto
                {
                    Count = x.Select(r => r.CustomerCard.CustomerId).Distinct().Count(),
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day
                }).ToListAsync();

            return GetCountAnalytic(metrics, date, granularity);
        }

        public async Task<CountAnalyticDto> GetRegistrationAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex, Granularity granularity)
        {
            var query = _integrationDataContext.CustomerSettings
                .AsQueryable();

            if (clientPlatform.HasValue)
            {
                var registrationPlatfrom = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                query = query.Where(x => x.Customer.RegistrationPlatform == registrationPlatfrom);
            }

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.Customer.Sex == sex);
            }

            var metrics = await query
                .Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo)
                .GroupBy(x => new { x.CreatedAt.Month, x.CreatedAt.Year, Day = granularity == Granularity.Day ? x.CreatedAt.Day : (int?)null })
                .Select(x => new CountMetricDto
                {
                    Count = x.Count(),
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day
                }).ToListAsync();

            return GetCountAnalytic(metrics, date, granularity);
        }

        public async Task<HourCountAnalyticDto> GetRegistrationHourAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.CustomerSettings
                .AsQueryable();

            if (clientPlatform.HasValue)
            {

                var registrationPlatfrom = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                query = query.Where(x => x.Customer.RegistrationPlatform == registrationPlatfrom);
            }

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.Customer.Sex == sex);
            }

            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            var metrics = await query
                .Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo)
                .GroupBy(x => x.CreatedAt.TimeOfDay.Hours)
                .Select(x => new HourCountMetricDto
                {
                    Count = x.Count(),
                    Hour = x.Key,
                }).ToListAsync();

            metrics = FillHourCountMetric(metrics);

            var analytic = new HourCountAnalyticDto
            {
                Metrics = metrics,
            };

            return analytic;
        }

        private List<HourCountMetricDto> FillHourCountMetric(List<HourCountMetricDto> metrics)
        {
            var missingMetrics = new List<HourCountMetricDto>();

            for (int i = 0; i < 24; i++)
            {
                if (!metrics.Any(x => x.Hour == i))
                {
                    missingMetrics.Add(new HourCountMetricDto
                    {
                        Hour = i,
                    });
                }
            }

            metrics.AddRange(missingMetrics);

            metrics = metrics.OrderBy(x => x.Hour).ToList();

            return metrics;
        }

        public async Task<List<WhereFindOutAnalyticMetric>> GetWhereFindOutAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date, RangeDTO<int?> age, Sex? sex)
        {
            // Use questionnaire data instead of Mouseflow
            // Include deleted customers to preserve their questionnaire data in analytics
            var query = GetBaseCustomerQuery(branchId, null, age, sex, includeDeleted: true);

            if (date.From.HasValue || date.To.HasValue)
            {
                var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);
                query = query.Where(x => x.PersonalQuestionaryCompletedAt.HasValue 
                    && x.PersonalQuestionaryCompletedAt.Value >= (dateTimeFrom ?? DateTime.MinValue)
                    && x.PersonalQuestionaryCompletedAt.Value <= (dateTimeTo ?? DateTime.MaxValue));
            }

            var metrics = await query
                .Where(x => !string.IsNullOrEmpty(x.WhereFindOut))
                .GroupBy(x => x.WhereFindOut)
                .Select(g => new
                {
                    Source = g.Key!,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalCount = metrics.Sum(x => x.Count);

            var results = metrics.Select(x => new WhereFindOutAnalyticMetric
            {
                Count = x.Count,
                Source = x.Source,
                Percent = totalCount > 0 ? Math.Round(100.0 * x.Count / totalCount, 2) : 0
            }).OrderByDescending(x => x.Count).ToList();

            return results;
        }

        private async Task GetAllRecordingsReferersAsync(string websiteId, DateTime datetimeFrom, DateTime datetimeTo, Dictionary<string, int> countDictionary)
        {
            var limit = 2000;
            var offset = 0;

            MouseflowRecordingListResponse batch;

            do
            {
                var response = await _mouseflowApi.GetWebsiteRecordingsListAsync(websiteId, offset, limit, datetimeFrom, datetimeTo);
                await response.EnsureSuccessfulAsync();
                batch = response.Content!;

                foreach (var recording in batch.Recordings)
                {
                    var excludeHosts = new string[] { new Uri(recording.Entry).Host, new Uri(_appSettings.BaseAdminUrl).Host };
                    var referrerHost = string.IsNullOrEmpty(recording.Referrer) ? "" : new Uri(recording.Referrer).Host;

                    if (excludeHosts.Contains(referrerHost))
                    {
                        countDictionary[recording.ReferrerType] = countDictionary.TryGetValue(recording.ReferrerType, out var count) ? count + 1 : 1;
                    }
                }

                offset += limit;
            }
            while (batch.Count == limit);
        }

        private IEnumerable<(int Year, Month Month, int? Day)> CreateGranularityKeys(RangeDTO<DateOnly> date, Granularity granularity)
        {
            return granularity switch
            {
                Granularity.Day => CreateGranularityDayKeys(date),
                Granularity.Month => CreateGranularityMonthKeys(date),
            };
        }

        private IEnumerable<(int Year, Month Month, int? Day)> CreateGranularityDayKeys(RangeDTO<DateOnly> date)
        {
            var iterationDate = date.From;

            do
            {
                yield return (iterationDate.Year, (Month)iterationDate.Month, iterationDate.Day);
                iterationDate = iterationDate.AddDays(1);
            } while (iterationDate <= date.To);
        }

        private IEnumerable<(int Year, Month Month, int? Day)> CreateGranularityMonthKeys(RangeDTO<DateOnly> date)
        {
            var iterationDate = new DateOnly(date.From.Year, date.From.Month, 1);
            var endDate = new DateOnly(date.To.Year, date.To.Month, 1);

            do
            {
                yield return (iterationDate.Year, (Month)iterationDate.Month, null);
                iterationDate = iterationDate.AddMonths(1);
            } while (iterationDate <= date.To);
        }

        private CountAnalyticDto GetCountAnalytic(List<CountMetricDto> metrics, RangeDTO<DateOnly> date, Granularity granularity)
        {
            metrics = FillMissingMetrics(metrics, date, granularity);

            var result = new CountAnalyticDto
            {
                Metrics = metrics,
            };

            return result;
        }

        private List<CountMetricDto> FillMissingMetrics(List<CountMetricDto> metrics, RangeDTO<DateOnly> date, Granularity granularity)
        {
            var missingMetrics = new List<CountMetricDto>();

            foreach (var (year, month, day) in CreateGranularityKeys(date, granularity))
            {
                if (!metrics.Exists(m => m.Year == year && m.Month == month && m.Day == day))
                {
                    missingMetrics.Add(new CountMetricDto
                    {
                        Year = year,
                        Month = month,
                        Day = day,
                        Count = 0,
                    });
                }
            }

            metrics = metrics.Concat(missingMetrics).OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).ToList();

            return metrics;
        }

        private static List<int> GetDaysInMonth(Month month)
        {
            int daysInMonth = month switch
            {
                Month.January => 31,
                Month.February => 29,
                Month.March => 31,
                Month.April => 30,
                Month.May => 31,
                Month.June => 30,
                Month.July => 31,
                Month.August => 31,
                Month.September => 30,
                Month.October => 31,
                Month.November => 30,
                Month.December => 31,
            };

            var monthKeys = Enumerable.Range(1, daysInMonth).ToList();
            return monthKeys;
        }

        private static (DateTime From, DateTime To) GetRangeDateTimes(RangeDTO<DateOnly> date)
        {
            if (date.From > date.To)
            {
                throw new ArgumentException("Date from must be less or equal than date to");
            }

            // Для "всього часу" (MinValue..MaxValue) використовуємо широкий UTC-діапазон без конвертації поясу,
            // щоб уникнути проблем з порівнянням дат у БД (наприклад, якщо CreatedAt зберігається як UTC або Unspecified).
            var isFullRange = date.From == DateOnly.MinValue && date.To == DateOnly.MaxValue;
            if (isFullRange)
            {
                return (
                    new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2100, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                );
            }

            // Clamp екстремальні значення до безпечного для NodaTime діапазону.
            var minSupported = new DateOnly(1900, 1, 1);
            var maxSupported = new DateOnly(2100, 1, 1);

            var safeFrom = date.From < minSupported ? minSupported : date.From;
            var safeTo = date.To > maxSupported ? maxSupported : date.To;

            var dateTimeFrom = safeFrom.ToDateTime(TimeOnly.MinValue).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            var dateTimeTo = safeTo.ToDateTime(TimeOnly.MinValue).AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);

            return (dateTimeFrom, dateTimeTo);
        }

        private static (DateTime? From, DateTime? To) GetRangeDateTimes(RangeDTO<DateOnly?> date)
        {
            if (date.From.HasValue && date.To.HasValue && date.From > date.To)
            {
                throw new ArgumentException("Date from must be less or equal than date to");
            }

            var dateTimeFrom = date.From?.ToDateTime(TimeOnly.MinValue).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            var dateTimeTo = date.To?.ToDateTime(TimeOnly.MinValue).AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);

            return (dateTimeFrom, dateTimeTo);
        }

        public async Task<List<DislocationMetricDto>> GetDislocationAnalyticAsync(byte? branchId, int? cityId, RangeDTO<int?> age, Sex? sex)
        {
            var storeQuery = _applicationDataContext.Stores.AsQueryable();

            if (branchId.HasValue)
            {
                storeQuery = storeQuery.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                storeQuery = storeQuery.Where(x => x.CityId == cityId);
            }

            var metrics = await storeQuery
                .Include(x => x.City)
                .Select(x => new DislocationMetricDto
                {
                    StoreId = x.Id,
                    StoreNumber = x.Number,
                    Address = x.Address,
                    Address2 = x.Address2,
                    City = x.City.Name,
                    CityId = x.CityId,
                }).ToListAsync();

            foreach (var metric in metrics)
            {
                var customerQuery = _integrationDataContext.Customers
                    .Where(x => !x.IsDeleted && x.StoreNearHomeId == metric.StoreId);

                var receiptQuery = _integrationDataContext.Receipts.Where(x => x.StoreCode == metric.StoreNumber);

                if (age.From.HasValue)
                {
                    customerQuery = customerQuery.Where(x => x.Birthday.HasValue && EF.Functions.DateDiffYear(x.Birthday, DateTime.UtcNow) >= age.From);
                    receiptQuery = receiptQuery.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
                }

                if (age.To.HasValue)
                {
                    customerQuery = customerQuery.Where(x => x.Birthday.HasValue && EF.Functions.DateDiffYear(x.Birthday, DateTime.UtcNow) <= age.To);
                    receiptQuery = receiptQuery.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
                }

                if (sex.HasValue)
                {
                    customerQuery = customerQuery.Where(x => x.Sex == sex);
                    receiptQuery = receiptQuery.Where(x => x.CustomerCard.Customer.Sex == sex);
                }

                metric.CustomerCount = await customerQuery.CountAsync();
                metric.NumberOfVisits = await receiptQuery.CountAsync();
            }

            return metrics;
        }

        public async Task<ReceiptAnalyticDto> GetReceiptAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var checkCount = await query.CountAsync();

            var quantity = await query.SelectMany(x => x.ReceiptProducts).SumAsync(x => x.Quantity);

            var amount = await query.SelectMany(x => x.ReceiptProducts).SumAsync(p => p.Price * p.Quantity - p.Discount);

            var result = new ReceiptAnalyticDto
            {
                Quantity = Math.Round(quantity, 2),
                AverageAmount = checkCount > 0 ? Math.Round(amount / checkCount, 2) : 0,
                AverageQuantity = checkCount > 0 ? Math.Round(quantity / checkCount, 2) : 0,
            };

            return result;
        }

        public async Task<PagedListDTO<ProductCategoryMetricDto>> GetProductCategoryAnalyticAsync(PagingDTO pagingDTO, byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => storeNumbers.Contains(x.StoreCode));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCode == storeNumber);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var productQuery = query
                .SelectMany(x => x.ReceiptProducts)
                .GroupBy(x => x.Product.Family.Category)
                .Select(x => new ProductCategoryMetricDto
                {
                    Amount = Math.Round(x.Sum(x => x.Price * x.Quantity), 2),
                    Quantity = Math.Round(x.Sum(x => x.Quantity), 2),
                    Code = x.Key.Code,
                    Title = x.Key.Title,
                });

            var items = await productQuery.GetPagedListAsync(pagingDTO, x => x);

            return items;
        }

        public async Task<ProgramUsageAnalytic> GetProgramUsageAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => storeNumbers.Contains(x.StoreCode));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCode == storeNumber);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }


            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var receiptMetricsWithCard = await query
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(x => new CountMetricDto
                {
                    Count = x.Count(),
                    Month = (Month)x.Key.Month,
                    Year = x.Key.Year,
                    Day = null,
                }).ToListAsync();

            receiptMetricsWithCard = FillMissingMetrics(receiptMetricsWithCard, date, Granularity.Month);
            var receiptMetricsWithoutCard = FillMissingMetrics(new List<CountMetricDto>(), date, Granularity.Month);

            var analytic = new ProgramUsageAnalytic
            {
                WithCardMetrics = receiptMetricsWithCard,
                WithoutCardMetrics = receiptMetricsWithoutCard,
            };

            return analytic;
        }

        public async Task<HourCountAnalyticDto> GetPurchaseTimeAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => storeNumbers.Contains(x.StoreCode));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCode == storeNumber);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }


            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var metrics = await query
                .GroupBy(x => x.CreatedAt.TimeOfDay.Hours)
                .Select(x => new HourCountMetricDto
                {
                    Hour = x.Key,
                    Count = x.Count(),
                })
                .ToListAsync();

            metrics = FillHourCountMetric(metrics);

            var analytic = new HourCountAnalyticDto
            {
                Metrics = metrics,
            };

            return analytic;
        }

        public async Task<GeneralCouponAnalyticDto> GetGeneralCouponAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.CustomerCoupons.AsQueryable();

            if (branchId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.BranchId == branchId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => s.StoreCode == storeNumber));
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.Customer.Sex == sex);
            }

            query = query.Where(x =>
                x.Coupon.StartDate >= dateFrom && x.Coupon.StartDate <= dateTo);

            var result = await query.GroupBy(x => true)
                .Select(x => new GeneralCouponAnalyticDto
                {
                    DistributedCount = x.Count(),
                    UsedCount = x.Count(s => s.UsedTimes == s.Coupon.UseTimes),
                }).FirstOrDefaultAsync();

            return result ?? new GeneralCouponAnalyticDto();
        }

        public async Task<PagedListDTO<CouponMetricDto>> GetCouponAnalyticAsync(PagingDTO pagingDTO, byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.Coupons.AsQueryable();

            if (branchId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.BranchId == branchId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCodes.Any(s => s.StoreCode == storeNumber));
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x =>
                x.StartDate >= dateFrom && x.StartDate <= dateTo);


            var resultQuery = query.Select(x => new CouponMetricDto
            {
                Id = x.Id,
                DistributedCount = x.CustomerCoupons.Count(
                    x => (!sex.HasValue || x.Customer.Sex == sex)
                    && (!age.From.HasValue || EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) >= age.From)
                    && (!age.To.HasValue || EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) <= age.To)),
                UsedCount = x.CustomerCoupons.Count(s => (!sex.HasValue || s.Customer.Sex == sex)
                    && (!age.From.HasValue || EF.Functions.DateDiffYear(s.Customer.Birthday, DateTime.UtcNow) >= age.From)
                    && (!age.To.HasValue || EF.Functions.DateDiffYear(s.Customer.Birthday, DateTime.UtcNow) <= age.To)
                    && s.UsedTimes == x.UseTimes),
                Name = x.Name,
            });

            var result = await resultQuery.GetPagedListAsync(pagingDTO, x => x);

            return result;
        }

        public async Task<FeedbackAnalyticDto> GetFeedbackAnalytic(byte? branchId, FeedbackType? type, int? storeId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform, RangeDTO<int?> age, Sex? sex)
        {
            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            var query = _applicationDataContext.Feedbacks.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (storeId.HasValue)
            {
                query = query.Where(x => x.StoreId == storeId);
            }

            if (type.HasValue)
            {
                query = query.Where(x => x.StoreId.HasValue == (type == FeedbackType.Store));
            }

            if (clientPlatform.HasValue)
            {
                query = query.Where(x => x.Platform == clientPlatform);
            }

            if (age.From.HasValue || age.To.HasValue || sex.HasValue)
            {
                var customerIds = await _integrationDataContext.Customers.Where(x =>
                    (!age.From.HasValue || (x.Birthday.HasValue && EF.Functions.DateDiffYear(x.Birthday, DateTime.UtcNow) >= age.From.Value))
                    && (!age.To.HasValue || (x.Birthday.HasValue && EF.Functions.DateDiffYear(x.Birthday, DateTime.UtcNow) <= age.To.Value))
                    && (!sex.HasValue || x.Sex == sex))
                    .Select(x => x.Id)
                    .ToListAsync();

                query = query.Where(x => customerIds.Contains(x.CustomerId));
            }

            var avgRating = await query.Select(x => (int)x.Rating).DefaultIfEmpty().AverageAsync();
            var avgMinutesUntilAnswer = await query
                    .Where(x => x.Status == FeedbackStatus.Completed)
                    .Select(x => (x.ProcessingDate!.Value - x.CreatedAt).TotalMinutes).DefaultIfEmpty().AverageAsync();

            var result = new FeedbackAnalyticDto
            {
                Count = await query.CountAsync(),
                AverageRating = Math.Round(avgRating, 2),
                AverageMinutesUntilAnswer = Math.Round(avgMinutesUntilAnswer, 2),
            };

            return result;
        }

        public async Task<List<TopicAnalyticMetric>> GetTopicAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date)
        {
            var query = _applicationDataContext.ContactUs.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            if (dateTimeFrom != null || dateTimeTo != null)
            {
                query = query.Where(s =>
                    (!dateTimeFrom.HasValue || s.CreatedAt >= dateTimeFrom)
                    && (!dateTimeTo.HasValue || s.CreatedAt < dateTimeTo));
            }

            var items = await query.GroupBy(x => x.TopicId!)
                .Select(x => new { TopicId = x.Key, Count = x.Count() })
                .ToListAsync();

            var totalCount = items.Sum(x => x.Count);

            var topics = await _applicationDataContext.Topics.AsQueryable().ToListAsync();

            var result = topics.Select(x => new TopicAnalyticMetric
            {
                Id = x.Id,
                Title = x.Title,
                Count = items.FirstOrDefault(i => i.TopicId == x.Id)?.Count ?? 0,
                Percent = Math.Round(totalCount != 0 ? items.FirstOrDefault(i => i.TopicId == x.Id)?.Count ?? 0 * 1d / totalCount : 0d, 2),
            }).ToList();

            return result;
        }

        public async Task<GeneralAccumulationCardAnalytic> GetGeneralAccumulationCardAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.CustomerAccumulationCards
                .Select(x => new CustomerAccumulationCardAndCouponItem
                {
                    Coupon = x.AccumulationCard.Coupons.First(),
                    CustomerAccumulationCard = x,
                })
                .AsQueryable();

            if (branchId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.BranchId == branchId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => s.StoreCode == storeNumber));
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerAccumulationCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerAccumulationCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerAccumulationCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerAccumulationCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerAccumulationCard.Customer.Sex == sex);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x =>
                x.Coupon.StartDate >= dateFrom
                && x.Coupon.StartDate <= dateTo
                && x.CustomerAccumulationCard.AccumulationCard.Status != Shared.Models.AccumulationCard.AccumulationCardStatus.Inactive);

            var result = await query.GroupBy(x => true)
                .Select(x => new GeneralAccumulationCardAnalytic
                {
                    DistributedCount = x.Count(),
                    ActiveCount = x.Count(x => x.CustomerAccumulationCard.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active
                        && x.CustomerAccumulationCard.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Active),
                    InactiveCount = x.Count(x => x.CustomerAccumulationCard.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active
                        && x.CustomerAccumulationCard.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Inactive),
                    ExpiredCount = x.Count(x => x.CustomerAccumulationCard.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Archived
                        && x.Coupon.ExpirationDate < x.CustomerAccumulationCard.AccumulationCard.UpdatedAt
                        && x.CustomerAccumulationCard.Status != Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Completed),
                    ArchivedCount = x.Count(x => x.CustomerAccumulationCard.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Archived
                        && x.Coupon.ExpirationDate >= x.CustomerAccumulationCard.AccumulationCard.UpdatedAt
                        && x.CustomerAccumulationCard.Status != Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Completed),
                    CompletedCount = x.Count(x => x.CustomerAccumulationCard.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Completed)
                })
                .FirstOrDefaultAsync();

            return result ?? new GeneralAccumulationCardAnalytic();
        }

        public async Task<PagedListDTO<AccumulationCardMetricDto>> GetAccumulationCardAnalyticAsync(PagingDTO pagingDTO, byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.AccumulationCards
                .Select(x => new AccumulationCardAndCouponItem
                {
                    AccumulationCard = x,
                    Coupon = x.Coupons.First(),
                })
                .AsQueryable();

            if (branchId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.BranchId == branchId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => s.StoreCode == storeNumber));
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x =>
                x.Coupon.StartDate >= dateFrom
                && x.Coupon.StartDate <= dateTo
                && x.AccumulationCard.Status != Shared.Models.AccumulationCard.AccumulationCardStatus.Inactive);

            var resultQuery = query
                .Select(x => new
                {
                    x.AccumulationCard, x.Coupon, CustomerAccumulationCards = x.AccumulationCard.CustomerAccumulationCards.Where(s => (!sex.HasValue || s.Customer.Sex == sex)
                    && (!age.From.HasValue || EF.Functions.DateDiffYear(s.Customer.Birthday, DateTime.UtcNow) >= age.From)
                    && (!age.To.HasValue || EF.Functions.DateDiffYear(s.Customer.Birthday, DateTime.UtcNow) <= age.To))
                })
                .Select(x => new AccumulationCardMetricDto
            {
                Id = x.AccumulationCard.Id,
                DistributedCount = x.CustomerAccumulationCards.Count(),
                ActiveCount = x.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active
                ? x.CustomerAccumulationCards.Count(x => x.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Active)
                : 0,
                InactiveCount = x.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active
                ? x.CustomerAccumulationCards.Count(x => x.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Inactive)
                : 0,
                ExpiredCount = x.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Archived && x.Coupon.ExpirationDate < x.AccumulationCard.UpdatedAt
                ? x.CustomerAccumulationCards.Count(x => x.Status != Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Completed)
                : 0,
                ArchivedCount = x.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Archived && x.Coupon.ExpirationDate >= x.AccumulationCard.UpdatedAt
                ? x.CustomerAccumulationCards.Count(x => x.Status != Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Completed)
                : 0,
                CompletedCount = x.CustomerAccumulationCards.Count(x => x.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Completed),
                Name = x.AccumulationCard.Name,
            });

            var result = await resultQuery.GetPagedListAsync(pagingDTO, x => x);

            return result;
        }

        public async Task<List<PageViewMetricDto>> GetPageVisitAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date)
        {
            var datetimeFrom = date.From.ToDateTime(TimeOnly.MinValue);
            var datetimeTo = date.To.ToDateTime(TimeOnly.MinValue).AddDays(1);

            var pages = new List<PageViewMetricDto>();

            if (!branchId.HasValue || branchId == (int)Branches.BirdJet)
            {
                var birdjetPages = await GetAllPagesAsync(_mouseflowSettings.BirdJetWebsiteId, datetimeFrom, datetimeTo);
                pages.AddRange(birdjetPages);
            }

            if (!branchId.HasValue || branchId == (int)Branches.CatJet)
            {
                var catjetPages = await GetAllPagesAsync(_mouseflowSettings.CatJetWebsiteId, datetimeFrom, datetimeTo);
                pages.AddRange(catjetPages);
            }

            // Після отримання даних з Mouseflow намагаємося підтягнути коректні тайтли з CMS-сторінок
            if (pages.Count > 0)
            {
                // Визначаємо, для яких філій потрібно підвантажити сторінки
                var branchIds = new List<byte>();

                if (!branchId.HasValue)
                {
                    branchIds.Add((byte)Branches.BirdJet);
                    branchIds.Add((byte)Branches.CatJet);
                }
                else
                {
                    // Використовуємо тільки відомі значення enum, інші ігноруємо
                    if (branchId == (byte)Branches.BirdJet || branchId == (byte)Branches.CatJet)
                    {
                        branchIds.Add(branchId.Value);
                    }
                }

                if (branchIds.Count > 0)
                {
                    var dbPages = await _applicationDataContext.Page
                        .Where(p => p.Link != null
                                    && p.IsActive
                                    && p.Published == true
                                    && p.BranchId.HasValue
                                    && branchIds.Contains(p.BranchId.Value))
                        .Select(p => new
                        {
                            p.Link,
                            p.Title
                        })
                        .ToListAsync();

                    if (dbPages.Count > 0)
                    {
                        foreach (var metric in pages)
                        {
                            if (string.IsNullOrWhiteSpace(metric.Uri) && string.IsNullOrWhiteSpace(metric.DisplayUrl))
                            {
                                continue;
                            }

                            var normalizedUri = NormalizePath(metric.Uri);
                            var normalizedDisplayUrl = NormalizePath(metric.DisplayUrl);

                            var matchedPage = dbPages.FirstOrDefault(p =>
                                NormalizePath(p.Link) == normalizedUri
                                || NormalizePath(p.Link) == normalizedDisplayUrl);

                            if (matchedPage != null && !string.IsNullOrWhiteSpace(matchedPage.Title))
                            {
                                // Перезаписуємо заголовок на той, що з CMS
                                metric.Title = matchedPage.Title;
                            }
                        }
                    }
                }

                // Ручний мапінг URL -> назва сторінки поверх Mouseflow/CMS.
                foreach (var metric in pages)
                {
                    if (string.IsNullOrWhiteSpace(metric.Uri) && string.IsNullOrWhiteSpace(metric.DisplayUrl))
                    {
                        continue;
                    }

                    var normalizedUri = NormalizePath(metric.Uri);
                    var normalizedDisplayUrl = NormalizePath(metric.DisplayUrl);

                    var key = !string.IsNullOrWhiteSpace(normalizedDisplayUrl)
                        ? normalizedDisplayUrl
                        : normalizedUri;

                    if (_pageTitlesByPath.TryGetValue(key, out var mappedTitle))
                    {
                        metric.Title = mappedTitle;
                    }
                }
            }

            pages = pages.OrderByDescending(x => x.Views).ToList();
            return pages;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var trimmed = path.Trim();

            // Якщо це повний URL — відрізаємо домен і залишаємо лише шлях.
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                trimmed = uri.AbsolutePath;
            }

            var withoutQuery = trimmed.Split('?', '#')[0];
            return withoutQuery.Trim().TrimEnd('/').ToLowerInvariant();
        }

        // Ручний мапінг нормалізованого шляху до назви сторінки
        private static readonly Dictionary<string, string> _pageTitlesByPath = new()
        {
            ["/"] = "Головна",
            ["/discount"] = "Знижка",
            ["/vouchers"] = "Що таке ваучери та купони",
            ["/holiday-offers"] = "Що таке святкові пропозиції",
            ["/promotions"] = "Акції",
            ["/charity"] = "Благодійність",
            ["/contacts"] = "Контакти",
            ["/policy"] = "Полісі",
            ["/cardplusone"] = "Карта накопичень",
            ["/cookies"] = "Cookies",
            ["/profile"] = "Профіль",
            ["/saved-promotions"] = "Збережені акції",
            ["/404"] = "404",

            ["/notifications"] = "Сповіщення",
            ["/settings"] = "Налаштування",
            ["/purchase-history"] = "Історія покупок",
            ["/wallet"] = "Гаманець",
            ["/discounts"] = "Знижки",
        };

        private async Task<List<PageViewMetricDto>> GetAllPagesAsync(string websiteId, DateTime datetimeFrom, DateTime datetimeTo)
        {
            var limit = 2000;
            var offset = 0;

            var result = new List<PageViewMetricDto>();

            List<MouseflowPageListItem> batch;

            do
            {
                var response = await _mouseflowApi.GetWebsitePageListAsync(websiteId, offset, limit, datetimeFrom, datetimeTo);
                await response.EnsureSuccessfulAsync();
                batch = response.Content!.Pages;

                var dtos = batch.Select(x => new PageViewMetricDto
                {
                    DisplayUrl = x.DisplayUrl,
                    Title = x.Title,
                    Uri = x.Uri,
                    Views = x.Views,
                    VisitTime = Math.Round(x.VisitTime / 60000d, 2),
                });

                result.AddRange(dtos);

                offset += limit;
            }
            while (batch.Count == limit);

            return result;
        }

        public async Task<CountAnalyticDto> GetTradeTurnoverAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex, Granularity granularity)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                var storeNumbersForBranch = await _applicationDataContext.Stores
                    .Where(s => s.BranchId == branchId.Value)
                    .Select(s => s.Number)
                    .ToListAsync();
                // Враховуємо чеки і по BranchId, і по StoreCode гілки (на випадок старих записів без BranchId або з іншої БД)
                query = query.Where(x => x.BranchId == branchId || (storeNumbersForBranch.Contains(x.StoreCode)));
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => storeNumbers.Contains(x.StoreCode));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCode == storeNumber);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var metrics = await query
                .SelectMany(x => x.ReceiptProducts)
                .GroupBy(x => new { x.Receipt.CreatedAt.Year, x.Receipt.CreatedAt.Month, Day = granularity == Granularity.Day ? x.Receipt.CreatedAt.Day : (int?)null })
                .Select(x => new CountMetricDto
                {
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day,
                    Count = (int)x.Sum(s => s.Quantity)
                }).ToListAsync();

            var analytic = GetCountAnalytic(metrics, date, granularity);
            return analytic;
        }

        public async Task<CustomerCountAnalyticDto> GetCustomerCountAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var query = _integrationDataContext.CustomerSettings
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.Customer.Sex == sex);
            }

            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            var registeredMetrics = await query
                .Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo)
                .GroupBy(x => new { x.CreatedAt.Month, x.CreatedAt.Year, Day = (int?)null })
                .Select(x => new CountMetricDto
                {
                    Count = x.Count(),
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day
                }).ToListAsync();

            var deletedMetrics = await query
                .Where(x => x.Customer.IsDeleted && x.Customer.UpdatedAt!.Value >= dateTimeFrom && x.Customer.UpdatedAt.Value < dateTimeTo)
                .GroupBy(x => new { x.Customer.UpdatedAt!.Value.Month, x.Customer.UpdatedAt!.Value.Year, Day = (int?)null })
                .Select(x => new CountMetricDto
                {
                    Count = x.Count(),
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day
                }).ToListAsync();

            var returnedMetrics = await query
                .SelectMany(x =>
                    _integrationDataContext.Receipts
                    .Where(r => r.CustomerCard.CustomerId == x.CustomerId
                        && r.BranchId == x.BranchId
                        && EF.Functions.DateFromParts(x.CreatedAt.Year, x.CreatedAt.Month, 1) > EF.Functions.DateFromParts(r.CreatedAt.Year, r.CreatedAt.Month, 1)
                        && x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo)
                    .Select(r => new { x.CustomerId, x.BranchId, r.CreatedAt.Year, r.CreatedAt.Month }))
                .GroupBy(x => new { x.CustomerId, x.BranchId, x.Year, x.Month })
                .Where(x => !_integrationDataContext.Receipts.Any(r => r.BranchId == x.Key.BranchId
                    && x.Key.CustomerId == r.CustomerCard.CustomerId
                    && EF.Functions.DateFromParts(x.Key.Year, x.Key.Month, 1) < r.CreatedAt
                    && EF.Functions.DateFromParts(x.Key.Year, x.Key.Month, 1).AddMonths(-1) < r.CreatedAt))
                .Select(x => new { x.Key.CustomerId, x.Key.BranchId, x.Key.Year, x.Key.Month })
                .GroupBy(x => new { x.Year, x.Month })
                .Select(x => new CountMetricDto
                {
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = null,
                    Count = x.Count()
                }).ToListAsync();

            deletedMetrics = FillMissingMetrics(deletedMetrics, date, Granularity.Month);
            registeredMetrics = FillMissingMetrics(registeredMetrics, date, Granularity.Month);
            returnedMetrics = FillMissingMetrics(returnedMetrics, date, Granularity.Month);

            var metrics = (from deleted in deletedMetrics
                           join registered in registeredMetrics on deleted.Key equals registered.Key
                           join returned in returnedMetrics on deleted.Key equals returned.Key
                           select new CustomerCountMetricDto
                           {
                               Day = deleted.Day,
                               Month = deleted.Month,
                               Year = deleted.Year,
                               DeletedCount = deleted.Count,
                               RegisteredCount = registered.Count,
                               ReturnedCount = returned.Count,
                           }).ToList();

            var analytic = new CustomerCountAnalyticDto
            {
                Metrics = metrics,
            };

            return analytic;
        }

        public async Task<GeneralRegistrationAnalyticDto> GetGeneralRegistrationAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform)
        {
            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            // Середній час реєстрації: від створення запису Customer до створення першого CustomerSettings (завершення реєстрації по гілці)
            var completedInRange = _integrationDataContext.CustomerSettings
                .Where(cs => cs.CreatedAt >= dateTimeFrom && cs.CreatedAt < dateTimeTo);
            if (branchId.HasValue)
                completedInRange = completedInRange.Where(cs => cs.BranchId == branchId);

            var completedWithCustomer = await completedInRange
                .GroupBy(cs => cs.CustomerId)
                .Select(g => new { CustomerId = g.Key, FirstSettingAt = g.Min(cs => cs.CreatedAt) })
                .ToListAsync();

            var customerIds = completedWithCustomer.Select(x => x.CustomerId).ToList();
            if (customerIds.Count == 0)
            {
                return new GeneralRegistrationAnalyticDto
                {
                    AverageRegistrationTime = 0,
                    CountLeftOvers = await GetRegistrationDropOffsCountAsync(dateTimeFrom, dateTimeTo, branchId, clientPlatform)
                };
            }

            var customerCreatedAt = await _integrationDataContext.Customers
                .Where(c => customerIds.Contains(c.Id))
                .Select(c => new { c.Id, c.CreatedAt })
                .ToDictionaryAsync(c => c.Id, c => c.CreatedAt);

            var timesMinutes = completedWithCustomer
                .Where(x => customerCreatedAt.TryGetValue(x.CustomerId, out var createdAt) && createdAt.HasValue)
                .Select(x => (x.FirstSettingAt - customerCreatedAt[x.CustomerId]!.Value).TotalMinutes)
                .Where(m => m >= 0)
                .ToList();

            var averageRegistrationTime = timesMinutes.Count > 0
                ? (decimal)timesMinutes.Average()
                : 0;

            var countLeftOvers = await GetRegistrationDropOffsCountAsync(dateTimeFrom, dateTimeTo, branchId, clientPlatform);

            return new GeneralRegistrationAnalyticDto
            {
                AverageRegistrationTime = Math.Round(averageRegistrationTime, 2),
                CountLeftOvers = countLeftOvers
            };
        }

        /// <summary>
        /// Той самий розрахунок, що GetGeneralRegistrationAnalyticAsync, але з діагностичними лічильниками (порівняти з SQL).
        /// </summary>
        public async Task<GeneralRegistrationAnalyticDebugDto> GetGeneralRegistrationAnalyticDebugAsync(byte? branchId, RangeDTO<DateOnly> date, ClientPlatform? clientPlatform)
        {
            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            var completedInRange = _integrationDataContext.CustomerSettings
                .Where(cs => cs.CreatedAt >= dateTimeFrom && cs.CreatedAt < dateTimeTo);
            if (branchId.HasValue)
                completedInRange = completedInRange.Where(cs => cs.BranchId == branchId);

            var debugSettingsInRange = await completedInRange.CountAsync();

            var completedWithCustomer = await completedInRange
                .GroupBy(cs => cs.CustomerId)
                .Select(g => new { CustomerId = g.Key, FirstSettingAt = g.Min(cs => cs.CreatedAt) })
                .ToListAsync();

            var debugCustomersWithSettingsInRange = completedWithCustomer.Count;
            var customerIds = completedWithCustomer.Select(x => x.CustomerId).ToList();

            if (customerIds.Count == 0)
            {
                var debugLeftOvers = await GetRegistrationDropOffsCountAsync(dateTimeFrom, dateTimeTo, branchId, clientPlatform);
                return new GeneralRegistrationAnalyticDebugDto
                {
                    AverageRegistrationTime = 0,
                    CountLeftOvers = debugLeftOvers,
                    DebugSettingsInRange = debugSettingsInRange,
                    DebugCustomersWithSettingsInRange = 0,
                    DebugCustomersWithCreatedAt = 0,
                    DebugLeftOvers = debugLeftOvers
                };
            }

            var customerCreatedAt = await _integrationDataContext.Customers
                .Where(c => customerIds.Contains(c.Id))
                .Select(c => new { c.Id, c.CreatedAt })
                .ToDictionaryAsync(c => c.Id, c => c.CreatedAt);

            var debugCustomersWithCreatedAt = completedWithCustomer.Count(x => customerCreatedAt.TryGetValue(x.CustomerId, out var createdAt) && createdAt.HasValue);

            var timesMinutes = completedWithCustomer
                .Where(x => customerCreatedAt.TryGetValue(x.CustomerId, out var createdAt) && createdAt.HasValue)
                .Select(x => (x.FirstSettingAt - customerCreatedAt[x.CustomerId]!.Value).TotalMinutes)
                .Where(m => m >= 0)
                .ToList();

            var averageRegistrationTime = timesMinutes.Count > 0
                ? (decimal)timesMinutes.Average()
                : 0;

            var countLeftOvers = await GetRegistrationDropOffsCountAsync(dateTimeFrom, dateTimeTo, branchId, clientPlatform);

            return new GeneralRegistrationAnalyticDebugDto
            {
                AverageRegistrationTime = Math.Round(averageRegistrationTime, 2),
                CountLeftOvers = countLeftOvers,
                DebugSettingsInRange = debugSettingsInRange,
                DebugCustomersWithSettingsInRange = debugCustomersWithSettingsInRange,
                DebugCustomersWithCreatedAt = debugCustomersWithCreatedAt,
                DebugLeftOvers = countLeftOvers
            };
        }

        /// <summary>
        /// Кількість користувачів, які створили запис (Customer) у період, але ніколи не завершили реєстрацію (немає жодного CustomerSettings).
        /// </summary>
        private async Task<int> GetRegistrationDropOffsCountAsync(DateTime dateTimeFrom, DateTime dateTimeTo, byte? branchId, ClientPlatform? clientPlatform)
        {
            var customersCreatedInRange = _integrationDataContext.Customers
                .Where(c => c.CreatedAt >= dateTimeFrom && c.CreatedAt < dateTimeTo && !c.IsDeleted);
            if (clientPlatform.HasValue)
            {
                var platform = clientPlatform.Value == ClientPlatform.App ? RegistrationPlatform.App : RegistrationPlatform.Web;
                customersCreatedInRange = customersCreatedInRange.Where(c => c.RegistrationPlatform == platform);
            }

            var withSettings = _integrationDataContext.CustomerSettings.AsQueryable();
            if (branchId.HasValue)
                withSettings = withSettings.Where(cs => cs.BranchId == branchId);

            var leftOvers = await customersCreatedInRange
                .Where(c => !withSettings.Any(cs => cs.CustomerId == c.Id))
                .CountAsync();
            return leftOvers;
        }

        public async Task<CardAndCouponUsageAnalyticByCategoryDTO> GetCardAndCouponUsageAnalyticByCategoryAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            var query = _integrationDataContext.Receipts.AsQueryable();

            query = query.Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo);

            if (branchId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.BranchId == branchId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => storeNumbers.Contains(x.StoreCode));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCode == storeNumber);
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var metricQuery = from r in query
                              from rp in r.ReceiptProducts
                              from rcc in r.ReceiptCustomerCoupons.Where(x => x.LineNo == rp.LineNo)
                              let cp = rcc.CustomerCoupon.Coupon
                              let cat = rp.Product.Family.Category
                              group new { cp, r } by new { cat.Code, cat.Title } into grouped
                              select new CardAndCouponUsageMetricDTO
                              {
                                  CategoryCode = grouped.Key.Code,
                                  CategoryTitle = grouped.Key.Title,
                                  CardUsages = grouped.Count(x => x.cp.IsCardCoupon),
                                  CouponUsages = grouped.Count(x => !x.cp.IsCardCoupon)
                              };

            var metrics = await metricQuery.ToListAsync();

            var analytic = new CardAndCouponUsageAnalyticByCategoryDTO
            {
                Metrics = metrics,
                TotalCardUsages = metrics.Sum(x => x.CardUsages),
                TotalCouponUsages = metrics.Sum(x => x.CouponUsages),
            };

            return analytic;
        }

        public async Task<CouponTimeUsageAnalyticDto> GetCouponTimeUsageAnalyticAsync(byte? branchId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex)
        {
            var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);

            var query = _integrationDataContext.CustomerCoupons
                .Where(x => x.UsedTimes == x.Coupon.UseTimes && x.UsedAt.HasValue && x.UsedAt >= dateTimeFrom && x.UsedAt < dateTimeTo)
                .AsQueryable();

            if (branchId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.BranchId == branchId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => x.Coupon.StoreCodes.Any(s => storeNumbers.Contains(s.StoreCode)));
            }

            if (age.From.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.Customer.Birthday, DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.Customer.Sex == sex);
            }

            var resultQuery = query.Select(x => EF.Functions.DateDiffSecond(x.UsedAt.Value, x.AssignedAt) / 60.0).DefaultIfEmpty();

            var minUsage = await resultQuery.MinAsync();
            var maxUsage = await resultQuery.MaxAsync();
            var avgUsage = await resultQuery.AverageAsync();

            var result = new CouponTimeUsageAnalyticDto
            {
                AverageMinutes = Math.Round(avgUsage, 2),
                MaxMinutes = Math.Round(avgUsage, 2),
                MinMinutes = Math.Round(minUsage, 2),
            };

            return result;
        }

        public async Task<ProgramSpentAnalyticDto> GetProgramSpentAnalyticAsync(byte? branchId, int? cityId, int? storeId, RangeDTO<DateOnly> date, RangeDTO<int?> age, Sex? sex, Granularity granularity)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                var storeNumbers = await _applicationDataContext.Stores.Where(x => x.CityId == cityId).Select(x => x.Number).ToListAsync();

                query = query.Where(x => storeNumbers.Contains(x.StoreCode));
            }

            if (storeId.HasValue)
            {
                var storeNumber = (await _applicationDataContext.Stores.FirstAsync(x => x.Id == storeId)).Number;

                query = query.Where(x => x.StoreCode == storeNumber);
            }

            var (dateFrom, dateTo) = GetRangeDateTimes(date);

            query = query.Where(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo);

            if (age.From.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= age.From);
            }

            if (age.To.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Birthday.HasValue && EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday ,DateTime.UtcNow) <= age.To);
            }

            if (sex.HasValue)
            {
                query = query.Where(x => x.CustomerCard.Customer.Sex == sex);
            }

            var metrics = await query
                .SelectMany(x => x.ReceiptProducts.Where(x => x.Discount > 0))
                .GroupBy(x => new { x.Receipt.CreatedAt.Year, x.Receipt.CreatedAt.Month, Day = granularity == Granularity.Day ? x.Receipt.CreatedAt.Day : (int?)null })
                .Select(x => new ProgramSpendAnalyticMetricDto
                {
                    Year = x.Key.Year,
                    Month = (Month)x.Key.Month,
                    Day = x.Key.Day,
                    Amount = Math.Round(x.Sum(x => x.Discount), 2),
                })
                .ToListAsync();

            var missingMetrics = new List<ProgramSpendAnalyticMetricDto>();

            foreach (var (year, month, day) in CreateGranularityKeys(date, granularity))
            {
                if (!metrics.Any(x => x.Year == year && x.Month == month && x.Day == day))
                {
                    missingMetrics.Add(new ProgramSpendAnalyticMetricDto
                    {
                        Year = year,
                        Month = month,
                        Day = day,
                        Amount = 0,
                    });
                }
            }

            metrics = metrics.Concat(missingMetrics).OrderBy(x => x.Key).ToList();

            var analytic = new ProgramSpentAnalyticDto
            {
                Metrics = metrics,
            };

            return analytic;
        }

        public async Task<RfmCustomerCountAnalyticDto> GetRfmCustomerCountAnalyticAsync(
            byte? branchId,
            int? cityId,
            int? storeId,
            RangeDTO<DateOnly> date,
            RangeDTO<int?> age,
            Sex? sex,
            Granularity granularity)
        {
            var allRfms = await _applicationDataContext.RFMs
                .AsNoTracking()
                .Include(r => r.Period)
                .Include(r => r.Amount)
                .Include(r => r.Count)
                .ToListAsync();

            if (allRfms.Count == 0)
            {
                return new RfmCustomerCountAnalyticDto { Series = new List<RfmSeriesDto>() };
            }

            var rfmMetaById = allRfms.ToDictionary(x => x.Id, x => (x.Name, x.Color));

            var timePoints = CreateGranularityKeys(date, granularity).ToList();

            if (timePoints.Count == 0)
            {
                return new RfmCustomerCountAnalyticDto { Series = new List<RfmSeriesDto>() };
            }

            var timePointRanges = timePoints
                .Select(tp =>
                {
                    var (year, month, day) = tp;
                    var startDate = day.HasValue
                        ? new DateOnly(year, (int)month, day.Value)
                        : new DateOnly(year, (int)month, 1);

                    var endDateExclusive = granularity == Granularity.Day
                        ? startDate.AddDays(1)
                        : startDate.AddMonths(1);

                    if (endDateExclusive > date.To.AddDays(1))
                    {
                        endDateExclusive = date.To.AddDays(1);
                    }

                    return (Key: tp, Start: startDate, EndExclusive: endDateExclusive);
                })
                .ToList();

            // Determine which dates need real-time queries based on Ukrainian timezone and job schedule
            var ukrainianTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.UATimezone);
            var nowUtc = DateTime.UtcNow;
            var nowUkrainian = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, ukrainianTimeZone);
            var todayUkrainian = DateOnly.FromDateTime(nowUkrainian);

            // Add 10-minute buffer to account for job execution time
            var jobScheduledTime = todayUkrainian.ToDateTime(new TimeOnly(JobConstants.RfmSnapshotJobHour, JobConstants.RfmSnapshotJobMinute));
            var jobCompletionTimeWithBuffer = jobScheduledTime.AddMinutes(10);

            // If current time is before job completion time + buffer, yesterday's snapshot hasn't been generated yet
            var jobHasRunToday = nowUkrainian >= jobCompletionTimeWithBuffer;

            var earliestRealTimeDate = jobHasRunToday
                ? todayUkrainian  // Job ran today, only today needs real-time
                : todayUkrainian.AddDays(-1);  // Job hasn't run yet, yesterday and today need real-time

            // Split ranges into historical (use snapshots), real-time (query receipts), and future (return zeros)
            var historicalRanges = timePointRanges.Where(r => r.Start < earliestRealTimeDate).ToList();
            var realTimeRanges = timePointRanges.Where(r => r.Start >= earliestRealTimeDate && r.Start <= todayUkrainian).ToList();

            var metrics = new List<RfmCustomerCountMetricDto>();

            var now = DateTime.UtcNow;

            var snapshotQuery = _integrationDataContext.RfmCustomerSnapshots.AsNoTracking();

            if (branchId.HasValue)
            {
                snapshotQuery = snapshotQuery.Where(x => x.BranchId == branchId);
            }

            if (cityId.HasValue)
            {
                snapshotQuery = snapshotQuery.Where(x => x.CityId == cityId);
            }

            if (storeId.HasValue)
            {
                snapshotQuery = snapshotQuery.Where(x => x.StoreId == storeId);
            }

            if (age.From.HasValue)
            {
                snapshotQuery = snapshotQuery.Where(x =>
                    x.Customer.Birthday.HasValue &&
                    EF.Functions.DateDiffYear(x.Customer.Birthday, now) >= age.From.Value);
            }

            if (age.To.HasValue)
            {
                snapshotQuery = snapshotQuery.Where(x =>
                    x.Customer.Birthday.HasValue &&
                    EF.Functions.DateDiffYear(x.Customer.Birthday, now) <= age.To.Value);
            }

            if (sex.HasValue)
            {
                snapshotQuery = snapshotQuery.Where(x => x.Customer.Sex == sex);
            }

            if (historicalRanges.Any())
            {
                var historicalStart = historicalRanges.First().Start;
                var historicalEndExclusive = historicalRanges.Last().EndExclusive;

                if (historicalEndExclusive > historicalStart)
                {
                    var historicalQuery = snapshotQuery
                        .Where(x => x.SnapshotDate >= historicalStart && x.SnapshotDate < historicalEndExclusive);

                    if (granularity == Granularity.Day)
                    {
                        var historicalData = await historicalQuery
                            .GroupBy(x => new { x.SnapshotDate, x.RfmId })
                            .Select(g => new
                            {
                                g.Key.SnapshotDate,
                                g.Key.RfmId,
                                Count = g.Count()
                            })
                            .ToListAsync();

                        foreach (var data in historicalData)
                        {
                            if (!rfmMetaById.TryGetValue(data.RfmId, out var meta))
                            {
                                continue;
                            }

                            metrics.Add(new RfmCustomerCountMetricDto
                            {
                                RfmId = data.RfmId,
                                RfmName = meta.Name,
                                RfmColor = meta.Color,
                                Year = data.SnapshotDate.Year,
                                Month = (Month)data.SnapshotDate.Month,
                                Day = data.SnapshotDate.Day,
                                CustomerCount = data.Count
                            });
                        }
                    }
                    else
                    {
                        var historicalData = await historicalQuery
                            .GroupBy(x => new { x.SnapshotDate, x.RfmId })
                            .Select(g => new
                            {
                                g.Key.SnapshotDate,
                                g.Key.RfmId,
                                Count = g.Count()
                            })
                            .ToListAsync();
                        var historicalDataPerMonth = historicalData
                            .GroupBy(x => new { x.SnapshotDate.Year, x.SnapshotDate.Month, x.RfmId })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                g.Key.RfmId,
                                Count = g.Max(x => x.Count)
                            })
                            .ToList();

                        foreach (var data in historicalDataPerMonth)
                        {
                            if (!rfmMetaById.TryGetValue(data.RfmId, out var meta))
                            {
                                continue;
                            }

                            metrics.Add(new RfmCustomerCountMetricDto
                            {
                                RfmId = data.RfmId,
                                RfmName = meta.Name,
                                RfmColor = meta.Color,
                                Year = data.Year,
                                Month = (Month)data.Month,
                                Day = null,
                                CustomerCount = data.Count
                            });
                        }
                    }
                }
            }

            // Process real-time ranges (today and potentially yesterday if job hasn't run)
            if (realTimeRanges.Any())
            {
                List<string>? cityStoreCodes = null;
                if (cityId.HasValue)
                {
                    cityStoreCodes = await _applicationDataContext.Stores
                        .Where(x => x.CityId == cityId.Value && (!branchId.HasValue || x.BranchId == branchId))
                        .Select(x => x.Number)
                        .ToListAsync();
                }

                string? storeCodeFilter = null;
                if (storeId.HasValue)
                {
                    storeCodeFilter = await _applicationDataContext.Stores
                        .Where(x => x.Id == storeId.Value &&
                                    (!cityId.HasValue || x.CityId == cityId.Value) &&
                                    (!branchId.HasValue || x.BranchId == branchId))
                        .Select(x => x.Number)
                        .FirstOrDefaultAsync();

                    if (storeCodeFilter == null)
                    {
                        return new RfmCustomerCountAnalyticDto { Series = new List<RfmSeriesDto>() };
                    }
                }

                // Process each real-time range separately
                foreach (var realTimeRange in realTimeRanges)
                {
                    var rangeEndExclusive = realTimeRange.EndExclusive;
                    var rangeEndDateTime = rangeEndExclusive.ToDateTime(TimeOnly.MinValue);

                    var baseReceiptQuery = BuildLocationFilteredReceiptQuery(branchId, cityStoreCodes, storeCodeFilter)
                        .AsNoTracking()
                        .Where(x => x.CreatedAt < rangeEndDateTime);

                    if (age.From.HasValue)
                    {
                        baseReceiptQuery = baseReceiptQuery.Where(x =>
                            x.CustomerCard.Customer.Birthday.HasValue &&
                            EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, now) >= age.From.Value);
                    }

                    if (age.To.HasValue)
                    {
                        baseReceiptQuery = baseReceiptQuery.Where(x =>
                            x.CustomerCard.Customer.Birthday.HasValue &&
                            EF.Functions.DateDiffYear(x.CustomerCard.Customer.Birthday, now) <= age.To.Value);
                    }

                    if (sex.HasValue)
                    {
                        baseReceiptQuery = baseReceiptQuery.Where(x => x.CustomerCard.Customer.Sex == sex);
                    }

                    var receiptTotals = await baseReceiptQuery
                        .Where(x => x.CustomerCard.CustomerId.HasValue)
                        .Select(x => new
                        {
                            CustomerId = x.CustomerCard.CustomerId!.Value,
                            x.CreatedAt,
                            Total = x.ReceiptProducts.Sum(p => (decimal?)(p.Price * p.Quantity - p.Discount)) ?? 0m
                        })
                        .ToListAsync();

                    var customerStats = receiptTotals
                        .GroupBy(x => x.CustomerId)
                        .Select(g => new
                        {
                            CustomerId = g.Key,
                            LastPurchase = g.Max(x => x.CreatedAt),
                            Frequency = g.Count(),
                            Monetary = g.Sum(x => x.Total)
                        })
                        .ToList();

                    var rangeCounts = new Dictionary<int, int>();

                    foreach (var stat in customerStats)
                    {
                        var recencyDays = (int)Math.Max(0, (rangeEndDateTime - stat.LastPurchase).TotalDays);
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

                        rangeCounts.TryGetValue(segment.Id, out var existing);
                        rangeCounts[segment.Id] = existing + 1;
                    }

                    foreach (var (rfmId, customerCount) in rangeCounts)
                    {
                        if (!rfmMetaById.TryGetValue(rfmId, out var meta))
                        {
                            continue;
                        }

                        metrics.Add(new RfmCustomerCountMetricDto
                        {
                            RfmId = rfmId,
                            RfmName = meta.Name,
                            RfmColor = meta.Color,
                            Year = realTimeRange.Key.Year,
                            Month = realTimeRange.Key.Month,
                            Day = realTimeRange.Key.Day,
                            CustomerCount = customerCount
                        });
                    }
                }
            }

            // Add zero metrics for RFMs with no customers at each time point (done in-memory)
            foreach (var (year, month, day) in timePoints)
            {
                var existingRfmIds = metrics
                    .Where(m => m.Year == year && m.Month == month && m.Day == day)
                    .Select(m => m.RfmId)
                    .ToHashSet();

                foreach (var rfm in allRfms.Where(r => !existingRfmIds.Contains(r.Id)))
                {
                    metrics.Add(new RfmCustomerCountMetricDto
                    {
                        RfmId = rfm.Id,
                        RfmName = rfm.Name,
                        RfmColor = rfm.Color,
                        Year = year,
                        Month = month,
                        Day = day,
                        CustomerCount = 0
                    });
                }
            }

            metrics = metrics
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Day ?? 0)
                .ThenBy(x => x.RfmId)
                .ToList();

            var series = metrics
                .GroupBy(m => m.RfmId)
                .Select(g => new RfmSeriesDto
                {
                    RfmId = g.Key,
                    RfmName = g.First().RfmName,
                    RfmColor = g.First().RfmColor,
                    Data = g.Select(m => new RfmTimePointDto
                    {
                        Day = m.Day,
                        Month = m.Month,
                        Year = m.Year,
                        CustomerCount = m.CustomerCount
                    }).ToList()
                })
                .OrderBy(s => s.RfmId)
                .ToList();

            return new RfmCustomerCountAnalyticDto
            {
                Series = series
            };
        }

        private IQueryable<Receipt> BuildLocationFilteredReceiptQuery(byte? branchId, IReadOnlyCollection<string>? cityStoreCodes, string? storeCode)
        {
            var query = _integrationDataContext.Receipts.AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (!string.IsNullOrEmpty(storeCode))
            {
                query = query.Where(x => x.StoreCode == storeCode);
            }
            else if (cityStoreCodes != null)
            {
                if (cityStoreCodes.Count == 0)
                {
                    return query.Where(x => false);
                }

                query = query.Where(x => cityStoreCodes.Contains(x.StoreCode));
            }

            return query;
        }

        private static string MapTypeOfActivityToDisplayName(CustomerTypeOfActivity type)
        {
            // Мапимо enum на українські назви, що використовуються в анкеті/адмінці
            return type switch
            {
                CustomerTypeOfActivity.Student    => "Студент",
                CustomerTypeOfActivity.Working    => "Працюю",
                CustomerTypeOfActivity.Unemployed => "Не працюю",
                CustomerTypeOfActivity.Pensioner  => "Пенсіонер",
                CustomerTypeOfActivity.Other      => "Інше",
                _                                 => type.ToString()
            };
        }

        public async Task<List<TypeOfActivityAnalyticMetric>> GetTypeOfActivityAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date, RangeDTO<int?> age, Sex? sex)
        {
            // Include deleted customers to preserve their questionnaire data in analytics
            var query = GetBaseCustomerQuery(branchId, null, age, sex, includeDeleted: true);

            if (date.From.HasValue || date.To.HasValue)
            {
                var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);
                query = query.Where(x => x.PersonalQuestionaryCompletedAt.HasValue 
                    && x.PersonalQuestionaryCompletedAt.Value >= (dateTimeFrom ?? DateTime.MinValue)
                    && x.PersonalQuestionaryCompletedAt.Value <= (dateTimeTo ?? DateTime.MaxValue));
            }

            var metrics = await query
                .Where(x => x.TypeOfActivity.HasValue)
                .GroupBy(x => x.TypeOfActivity)
                .Select(g => new
                {
                    TypeOfActivity = g.Key!.Value,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalCount = metrics.Sum(x => x.Count);

            var results = metrics.Select(x => new TypeOfActivityAnalyticMetric
            {
                // Повертаємо людиночитну назву, а не сирий enum-ключ
                TypeOfActivity = MapTypeOfActivityToDisplayName(x.TypeOfActivity),
                Count = x.Count,
                Percent = totalCount > 0 ? Math.Round(100.0 * x.Count / totalCount, 2) : 0
            }).OrderByDescending(x => x.Count).ToList();

            return results;
        }

        public async Task<List<NumberOfChildrenAnalyticMetric>> GetNumberOfChildrenAnalyticAsync(byte? branchId, RangeDTO<DateOnly?> date, RangeDTO<int?> age, Sex? sex)
        {
            // Include deleted customers to preserve their questionnaire data in analytics
            var query = GetBaseCustomerQuery(branchId, null, age, sex, includeDeleted: true);

            if (date.From.HasValue || date.To.HasValue)
            {
                var (dateTimeFrom, dateTimeTo) = GetRangeDateTimes(date);
                query = query.Where(x => x.PersonalQuestionaryCompletedAt.HasValue 
                    && x.PersonalQuestionaryCompletedAt.Value >= (dateTimeFrom ?? DateTime.MinValue)
                    && x.PersonalQuestionaryCompletedAt.Value <= (dateTimeTo ?? DateTime.MaxValue));
            }

            var metrics = await query
                .Where(x => x.NumberOfChildren.HasValue)
                .GroupBy(x => x.NumberOfChildren)
                .Select(g => new
                {
                    NumberOfChildren = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalCount = metrics.Sum(x => x.Count);

            var results = metrics.Select(x => new NumberOfChildrenAnalyticMetric
            {
                NumberOfChildren = x.NumberOfChildren,
                Count = x.Count,
                Percent = totalCount > 0 ? Math.Round(100.0 * x.Count / totalCount, 2) : 0
            }).OrderBy(x => x.NumberOfChildren).ToList();

            return results;
        }
    }
}
