using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Analytic;
using JetFlight.Shared.Models.Targets;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Feedback;
using JetFlight.Shared.Models.Mouseflow;
using JetFlight.Shared.Models.Analytic.Export;
using JetFlight.Shared.Models.Users;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class AnalyticController : BaseController
    {
        private readonly IAnalyticService _analyticService;
        private readonly IAnalyticExportService _analyticExportService;
        private readonly IRfmSnapshotService _rfmSnapshotService;

        public AnalyticController(IAnalyticService analyticService, IAnalyticExportService analyticExportService, IRfmSnapshotService rfmSnapshotService)
        {
            _analyticService = analyticService;
            _analyticExportService = analyticExportService;
            _rfmSnapshotService = rfmSnapshotService;
        }


        [ProducesResponseType(typeof(CountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetActiveCustomerDayAnalytic(byte? branchId, DateOnly dateFrom, DateOnly dateTo, ClientPlatform? clientPlatform, int? ageFrom, int? ageTo, Sex? sex, Granularity granularity)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetActiveCustomerAnalyticAsync(branchId, date, clientPlatform, ageRange, sex, granularity));
        }

        [ProducesResponseType(typeof(AgeAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAgeAnalytic(byte? branchId, int? cityId, int? activeStoreId, ClientPlatform? clientPlatform, int? ageFrom, int? ageTo, Sex? sex)
        {
            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetAgeAnalyticAsync(branchId, cityId, activeStoreId, clientPlatform, ageRange, sex));
        }

        [ProducesResponseType(typeof(BirthdayAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetBirthdayAnalytic(byte? branchId, int? cityId, int? activeStoreId, Month? month, ClientPlatform? clientPlatform, int? ageFrom, int? ageTo, Sex? sex)
        {
            var monthRange = new RangeDTO<Month>
            {
                From = month ?? Month.January,
                To = month ?? Month.December,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            var granularity = month.HasValue ? Granularity.Day : Granularity.Month;

            return Ok(await _analyticService.GetBirthdayAnalyticAsync(branchId, cityId, activeStoreId, monthRange, clientPlatform, ageRange, sex, granularity));
        }

        [ProducesResponseType(typeof(CustomerGeneralAnalyticsDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetGeneralAnalytics(byte? branchId, int? cityId, int? activeStoreId, ClientPlatform? clientPlatform, int? ageFrom, int? ageTo, Sex? sex)
        {
            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetGeneralAnalyticsAsync(branchId, cityId, activeStoreId, clientPlatform, ageRange, sex));
        }

        [ProducesResponseType(typeof(CountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetRegistrationAnalytic(byte? branchId, DateOnly dateFrom, DateOnly dateTo, ClientPlatform? clientPlatform, Granularity granularity, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetRegistrationAnalyticAsync(branchId, date, clientPlatform, ageRange, sex, granularity));
        }

        [ProducesResponseType(typeof(HourCountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetRegistrationHourAnalytic(byte? branchId, DateOnly dateFrom, DateOnly dateTo, ClientPlatform? clientPlatform, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetRegistrationHourAnalyticAsync(branchId, date, clientPlatform, ageRange, sex));
        }

        [ProducesResponseType(typeof(List<WhereFindOutAnalyticMetric>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetWhereFindOutAnalytic(byte? branchId, DateOnly? dateFrom, DateOnly? dateTo, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly?>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetWhereFindOutAnalyticAsync(branchId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(List<TypeOfActivityAnalyticMetric>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetTypeOfActivityAnalytic(byte? branchId, DateOnly? dateFrom, DateOnly? dateTo, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly?>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetTypeOfActivityAnalyticAsync(branchId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(List<NumberOfChildrenAnalyticMetric>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetNumberOfChildrenAnalytic(byte? branchId, DateOnly? dateFrom, DateOnly? dateTo, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly?>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetNumberOfChildrenAnalyticAsync(branchId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(List<DislocationMetricDto>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetDislocationAnalytic(byte? branchId, int? cityId, int? ageFrom, int? ageTo, Sex? sex)
        {
            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetDislocationAnalyticAsync(branchId, cityId, ageRange, sex));
        }

        [ProducesResponseType(typeof(ReceiptAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetReceiptAnalytic(byte? branchId, DateOnly dateFrom, DateOnly dateTo, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetReceiptAnalyticAsync(branchId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(PagedListDTO<ProductCategoryMetricDto>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductCategoryAnalytic(
            int? offset,
            int? limit,
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(ProductCategoryMetricDto), "Code", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetProductCategoryAnalyticAsync(validatePagingParamsDTO.PagingDTO, branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(HourCountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetPurchaseTimeAnalytic(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetPurchaseTimeAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(GeneralCouponAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetGeneralCouponAnalyticAsync(
            int? offset,
            int? limit,
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetGeneralCouponAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(PagedListDTO<CouponMetricDto>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetCouponAnalyticAsync(
            int? offset,
            int? limit,
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(CouponMetricDto), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetCouponAnalyticAsync(validatePagingParamsDTO.PagingDTO, branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(FeedbackAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetFeedbackAnalytic(byte? branchId, FeedbackType? type, int? storeId, DateOnly dateFrom, DateOnly dateTo, ClientPlatform? clientPlatform, int? ageFrom, int? ageTo, Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetFeedbackAnalytic(branchId, type, storeId, date, clientPlatform, ageRange, sex));
        }

        [ProducesResponseType(typeof(List<TopicAnalyticMetric>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetTopicAnalytic(byte? branchId, DateOnly? dateFrom, DateOnly? dateTo)
        {
            var date = new RangeDTO<DateOnly?>
            {
                From = dateFrom,
                To = dateTo,
            };

            return Ok(await _analyticService.GetTopicAnalyticAsync(branchId, date));
        }

        [ProducesResponseType(typeof(GeneralAccumulationCardAnalytic), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetGeneralAccumulationCardAnalytic(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetGeneralAccumulationCardAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(PagedListDTO<AccumulationCardMetricDto>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAccumulationCardAnalytic(
            int? offset,
            int? limit,
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(AccumulationCardMetricDto), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetAccumulationCardAnalyticAsync(validatePagingParamsDTO.PagingDTO, branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(ProgramUsageAnalytic), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetProgramUsageAnalytic(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetProgramUsageAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(List<PageViewMetricDto>), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetPageVisitAnalytic(
            byte? branchId,
            DateOnly dateFrom,
            DateOnly dateTo)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };


            return Ok(await _analyticService.GetPageVisitAnalyticAsync(branchId, date));
        }

        [ProducesResponseType(typeof(CountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetTradeTurnoverAnalytic(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex,
            Granularity granularity)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetTradeTurnoverAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex, granularity));
        }

        [ProducesResponseType(typeof(CustomerCountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetCustomerCountAnalytic(
            byte? branchId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetCustomerCountAnalyticAsync(branchId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(GeneralRegistrationAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetGeneralRegistrationAnalytic(
            byte? branchId,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            ClientPlatform? clientPlatform)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom ?? DateOnly.MinValue,
                To = dateTo ?? DateOnly.MaxValue,
            };

            return Ok(await _analyticService.GetGeneralRegistrationAnalyticAsync(branchId, date, clientPlatform));
        }

        /// <summary>
        /// Діагностика: ті самі параметри, що GetGeneralRegistrationAnalytic, але у відповіді додаткові лічильники (порівняти з SQL).
        /// </summary>
        [ProducesResponseType(typeof(GeneralRegistrationAnalyticDebugDto), 200)]
        [HttpGet("[action]Debug")]
        public async Task<IActionResult> GetGeneralRegistrationAnalyticDebug(
            byte? branchId,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            ClientPlatform? clientPlatform)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom ?? DateOnly.MinValue,
                To = dateTo ?? DateOnly.MaxValue,
            };
            return Ok(await _analyticService.GetGeneralRegistrationAnalyticDebugAsync(branchId, date, clientPlatform));
        }

        [ProducesResponseType(typeof(CardAndCouponUsageAnalyticByCategoryDTO), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetCardAndCouponUsageAnalyticByCategory(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetCardAndCouponUsageAnalyticByCategoryAsync(branchId, cityId, storeId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(CouponTimeUsageAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetCouponTimeUsageAnalytic(
            byte? branchId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetCouponTimeUsageAnalyticAsync(branchId, date, ageRange, sex));
        }

        [ProducesResponseType(typeof(ProgramSpentAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetProgramSpentAnalytic(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex,
            Granularity granularity)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetProgramSpentAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex, granularity));
        }

        [ProducesResponseType(typeof(RfmCustomerCountAnalyticDto), 200)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetRfmCustomerCountAnalytic(
            byte? branchId,
            int? cityId,
            int? storeId,
            DateOnly dateFrom,
            DateOnly dateTo,
            int? ageFrom,
            int? ageTo,
            Sex? sex,
            Granularity granularity)
        {
            var date = new RangeDTO<DateOnly>
            {
                From = dateFrom,
                To = dateTo,
            };

            var ageRange = new RangeDTO<int?>
            {
                From = ageFrom,
                To = ageTo,
            };

            return Ok(await _analyticService.GetRfmCustomerCountAnalyticAsync(branchId, cityId, storeId, date, ageRange, sex, granularity));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> TriggerRFMSnapshotJob([FromQuery] int? daysBack = 90)
        {
            try
            {
                var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)); // Yesterday
                var startDate = endDate.AddDays(-(daysBack ?? 90));

                var results = new List<string>();

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    await _rfmSnapshotService.GenerateDailySnapshotAsync(date, CancellationToken.None);
                    results.Add($"Generated snapshot for {date:yyyy-MM-dd}");
                }

                return Ok(new
                {
                    message = $"Successfully generated {results.Count} RFM snapshots",
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd"),
                    details = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

       [ProducesResponseType(typeof(FileResult), 200)]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetGeneralAnalyticExportFile(
            GeneralAnalyticExportFiltersDto filters)
        {
            var exportFile = await _analyticExportService.GetGeneralAnalyticExportFileAsync(filters);
            return File(exportFile.Stream, exportFile.ContentType, exportFile.FileName);
        }

        [ProducesResponseType(typeof(FileResult), 200)]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetRegistrationAnalyticExportFile(
            RegistrationAnalyticExportFiltersDto filters)
        {
            var exportFile = await _analyticExportService.GetRegistrationAnalyticExportFileAsync(filters);
            return File(exportFile.Stream, exportFile.ContentType, exportFile.FileName);
        }

        [ProducesResponseType(typeof(FileResult), 200)]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetProductAnalyticExportFile(
            ProductAnalyticExportFiltersDto filters)
        {
            var exportFile = await _analyticExportService.GetProductAnalyticExportFileAsync(filters);
            return File(exportFile.Stream, exportFile.ContentType, exportFile.FileName);
        }

        [ProducesResponseType(typeof(FileResult), 200)]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetLoyaltyAnalyticExportFile(
            LoyaltyAnalyticExportFiltersDto filters)
        {
            var exportFile = await _analyticExportService.GetLoyaltyAnalyticExportFileAsync(filters);
            return File(exportFile.Stream, exportFile.ContentType, exportFile.FileName);
        }

        [ProducesResponseType(typeof(FileResult), 200)]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetFeedbackAnalyticExportFile(
            FeedbackAnalyticExportFiltersDto filters)
        {
            var exportFile = await _analyticExportService.GetFeedbackAnalyticExportFileAsync(filters);
            return File(exportFile.Stream, exportFile.ContentType, exportFile.FileName);
        }
    }
}
