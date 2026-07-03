using ClosedXML.Excel;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Analytic;
using JetFlight.Shared.Models.Analytic.Export;
using JetFlight.Shared.Models.Export;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Targets;

namespace JetFlight.Service.Services
{
    public interface IAnalyticExportService
    {
        Task<ExportFile> GetFeedbackAnalyticExportFileAsync(FeedbackAnalyticExportFiltersDto filters);
        Task<ExportFile> GetGeneralAnalyticExportFileAsync(GeneralAnalyticExportFiltersDto filters);
        Task<ExportFile> GetLoyaltyAnalyticExportFileAsync(LoyaltyAnalyticExportFiltersDto filters);
        Task<ExportFile> GetProductAnalyticExportFileAsync(ProductAnalyticExportFiltersDto filters);
        Task<ExportFile> GetRegistrationAnalyticExportFileAsync(RegistrationAnalyticExportFiltersDto filters);
    }

    public class AnalyticExportService : IAnalyticExportService
    {
        private readonly IAnalyticService _analyticService;

        public AnalyticExportService(IAnalyticService analyticService)
        {
            _analyticService = analyticService;
        }

        public async Task<ExportFile> GetGeneralAnalyticExportFileAsync(GeneralAnalyticExportFiltersDto filters)
        {
            using var workbook = new XLWorkbook();

            await AddGeneralCustomerSheetAsync(filters, workbook);
            await AddCustomerAgeSheetAsync(filters, workbook);
            await AddCustomerBirthdaySheetAsync(filters, workbook);
            await AddCustomerDislocationSheetAsync(filters, workbook);
            await AddPageViewSheetAsync(filters, workbook);
            await AddActiveCustomersSheetAsync(filters, workbook);
            await AddWhereFindOutSheedAsync(filters, workbook);
            await AddTypeOfActivitySheetAsync(filters, workbook);
            await AddNumberOfChildrenSheetAsync(filters, workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"Загальна-аналітика-{DateTimeOffset.UtcNow.Ticks}.xlsx"
            };
        }

        private async Task AddGeneralCustomerSheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Загальна інформація");

            var analytic = await _analyticService.GetGeneralAnalyticsAsync(
                filters.GeneralCustomerFilters.BranchId,
                filters.GeneralCustomerFilters.CityId,
                filters.GeneralCustomerFilters.ActiveStoreId,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Всього користувачів";
            worksheet.Cell(1, 2).Value = "Чоловіки";
            worksheet.Cell(1, 3).Value = "Жінки";

            worksheet.Cell(2, 1).Value = CreateCell(analytic.Count);
            worksheet.Cell(2, 2).Value = CreateCell(analytic.MaleCount);
            worksheet.Cell(2, 3).Value = CreateCell(analytic.FemaleCount);
        }

        private async Task AddCustomerAgeSheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Користувачі - вік");

            var analytic = await _analyticService.GetAgeAnalyticAsync(
                filters.GeneralCustomerFilters.BranchId,
                filters.GeneralCustomerFilters.CityId,
                filters.GeneralCustomerFilters.ActiveStoreId,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Вік";
            worksheet.Cell(1, 2).Value = "Кількість";

            var index = 2;
            foreach (var metric in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.Age);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Count);
                index++;
            }
        }

        private async Task AddCustomerBirthdaySheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Дні народження");

            var granularity = filters.CustomerBirthdayFilters.Month.HasValue ? Granularity.Day : Granularity.Month;
            var monthRange = new RangeDTO<Month>
            {
                From = filters.CustomerBirthdayFilters.Month ?? Month.January,
                To = filters.CustomerBirthdayFilters.Month ?? Month.December,
            };

            var analytic = await _analyticService.GetBirthdayAnalyticAsync(
                filters.CustomerBirthdayFilters.BranchId,
                filters.CustomerBirthdayFilters.CityId,
                filters.CustomerBirthdayFilters.ActiveStoreId,
                monthRange,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex,
                granularity);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 2).Value = "Кількість";


            var index = 2;
            foreach (var metric in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.Day);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Month.ToString());
                worksheet.Cell(index, 3).Value = CreateCell(metric.Count);
                index++;
            }
        }

        private async Task AddCustomerDislocationSheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Місце знаходження");

            var metrics = await _analyticService.GetDislocationAnalyticAsync(
                filters.CustomerDislocationFilter.BranchId,
                filters.CustomerDislocationFilter.CityId,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Вулиця";
            worksheet.Cell(1, 2).Value = "Місто";
            worksheet.Cell(1, 3).Value = "Кількість користувачів";
            worksheet.Cell(1, 4).Value = "Кількість відвідувань";


            var index = 2;
            foreach (var metric in metrics)
            {
                worksheet.Cell(index, 1).Value = $"{metric.Address} {metric.Address2}";
                worksheet.Cell(index, 2).Value = metric.City;
                worksheet.Cell(index, 3).Value = metric.CustomerCount;
                worksheet.Cell(index, 4).Value = metric.NumberOfVisits;
                index++;
            }
        }

        private async Task AddPageViewSheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Сторінки");

            var metrics = await _analyticService.GetPageVisitAnalyticAsync(
                filters.PageViewFilters.BranchId,
                filters.PageViewFilters.Date);

            worksheet.Cell(1, 1).Value = "DisplayUrl";
            worksheet.Cell(1, 2).Value = "Uri";
            worksheet.Cell(1, 3).Value = "Title";
            worksheet.Cell(1, 4).Value = "Views";
            worksheet.Cell(1, 5).Value = "VisitTime (хв)";


            var index = 2;
            foreach (var metric in metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.DisplayUrl);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Uri);
                worksheet.Cell(index, 3).Value = CreateCell(metric.Title);
                worksheet.Cell(index, 4).Value = CreateCell(metric.Views);
                worksheet.Cell(index, 5).Value = CreateCell(metric.VisitTime);
                index++;
            }
        }

        private async Task AddActiveCustomersSheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Активні користувачі");

            var analyticDto = await _analyticService.GetActiveCustomerAnalyticAsync(
                filters.ActiveCustomerFilters.BranchId,
                filters.ActiveCustomerFilters.Date,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex,
                filters.ActiveCustomerFilters.Granularity);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 2).Value = "Кількість";


            var index = 2;
            foreach (var metric in analyticDto.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.Day);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Month.ToString());
                worksheet.Cell(index, 3).Value = CreateCell(metric.Count);
                index++;
            }
        }

        private async Task AddWhereFindOutSheedAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Джерело залученості");

            var metrics = await _analyticService.GetWhereFindOutAnalyticAsync(
                filters.WhereFindOutFilter.BranchId,
                filters.WhereFindOutFilter.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Джерело";
            worksheet.Cell(1, 2).Value = "Кількість";
            worksheet.Cell(1, 3).Value = "Відсоток";


            var index = 2;
            foreach (var metric in metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.Source);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Count);
                worksheet.Cell(index, 3).Value = CreateCell(metric.Percent);
                index++;
            }
        }

        private async Task AddTypeOfActivitySheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add(PersonalDataQuestionaryConstants.TypeOfActivityField);

            var metrics = await _analyticService.GetTypeOfActivityAnalyticAsync(
                filters.TypeOfActivityFilter.BranchId,
                filters.TypeOfActivityFilter.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = PersonalDataQuestionaryConstants.TypeOfActivityField;
            worksheet.Cell(1, 2).Value = "Кількість";
            worksheet.Cell(1, 3).Value = "Відсоток";

            var index = 2;
            foreach (var metric in metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.TypeOfActivity);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Count);
                worksheet.Cell(index, 3).Value = CreateCell(metric.Percent);
                index++;
            }
        }

        private async Task AddNumberOfChildrenSheetAsync(GeneralAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Кількість дітей");

            var metrics = await _analyticService.GetNumberOfChildrenAnalyticAsync(
                filters.NumberOfChildrenFilter.BranchId,
                filters.NumberOfChildrenFilter.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Кількість дітей";
            worksheet.Cell(1, 2).Value = "Кількість";
            worksheet.Cell(1, 3).Value = "Відсоток";

            var index = 2;
            foreach (var metric in metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(metric.NumberOfChildren);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Count);
                worksheet.Cell(index, 3).Value = CreateCell(metric.Percent);
                index++;
            }
        }

        public async Task<ExportFile> GetRegistrationAnalyticExportFileAsync(RegistrationAnalyticExportFiltersDto filters)
        {
            using var workbook = new XLWorkbook();

            await AddGeneralRegistionSheetAsync(filters, workbook);
            await AddRegistrationSheetAsync(filters, workbook);
            await AddRegistrationHoursSheetAsync(filters, workbook);
            await AddCustomerCountSheetAsync(filters, workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"Рєєстрація-{DateTimeOffset.UtcNow.Ticks}.xlsx"
            };
        }

        private async Task AddGeneralRegistionSheetAsync(RegistrationAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Загальна інформація");

            var analytic = await _analyticService.GetGeneralRegistrationAnalyticAsync(
                null,
                new RangeDTO<DateOnly>
                {
                    From = DateOnly.MinValue,
                    To = DateOnly.MaxValue
                },
                filters.ClientPlatform);

            worksheet.Cell(1, 1).Value = "Середній час на реєстрацію (хв)";
            worksheet.Cell(1, 2).Value = "Покинули рєєстрацію";

            worksheet.Cell(2, 1).Value = analytic.AverageRegistrationTime;
            worksheet.Cell(2, 2).Value = analytic.CountLeftOvers;
        }

        private async Task AddRegistrationSheetAsync(RegistrationAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Кількість рєєстрацій");

            var analytic = await _analyticService.GetRegistrationAnalyticAsync(
                filters.RegistrationFilters.BranchId,
                filters.RegistrationFilters.Date,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex,
                filters.RegistrationFilters.Granularity);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 3).Value = "Рік";
            worksheet.Cell(1, 4).Value = "Кількість";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Day);
                worksheet.Cell(index, 2).Value = CreateCell(item.Month.ToString());
                worksheet.Cell(index, 3).Value = CreateCell(item.Year);
                worksheet.Cell(index, 4).Value = CreateCell(item.Count);

                index++;
            }
        }

        private async Task AddRegistrationHoursSheetAsync(RegistrationAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Час рєєстрації");

            var analytic = await _analyticService.GetRegistrationHourAnalyticAsync(
                filters.RegistrationHoursFilters.BranchId,
                filters.RegistrationHoursFilters.Date,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Година";
            worksheet.Cell(1, 4).Value = "Кількість";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Hour);
                worksheet.Cell(index, 2).Value = CreateCell(item.Count);

                index++;
            }
        }

        private async Task AddCustomerCountSheetAsync(RegistrationAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Кількість клієнтів");

            var analytic = await _analyticService.GetCustomerCountAnalyticAsync(
                filters.CustomerCountFilters.BranchId,
                filters.CustomerCountFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 3).Value = "Рік";
            worksheet.Cell(1, 4).Value = "Нових";
            worksheet.Cell(1, 5).Value = "Повернених";
            worksheet.Cell(1, 6).Value = "Видалений";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Day);
                worksheet.Cell(index, 2).Value = CreateCell(item.Month.ToString());
                worksheet.Cell(index, 3).Value = CreateCell(item.Year);
                worksheet.Cell(index, 4).Value = CreateCell(item.RegisteredCount);
                worksheet.Cell(index, 5).Value = CreateCell(item.ReturnedCount);
                worksheet.Cell(index, 6).Value = CreateCell(item.DeletedCount);

                index++;
            }
        }

        public async Task<ExportFile> GetProductAnalyticExportFileAsync(ProductAnalyticExportFiltersDto filters)
        {
            using var workbook = new XLWorkbook();

            await AddReceiptSheetAsync(filters, workbook);
            await AddProductCategorySheetAsync(filters, workbook);
            await AddTradeTurnoverSheetAsync(filters, workbook);
            await AddPurchaseTimeSheetAsync(filters, workbook);
            await AddCardAndCouponUsageSheetAsync(filters, workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"Товар-{DateTimeOffset.UtcNow.Ticks}.xlsx"
            };
        }

        private async Task AddReceiptSheetAsync(ProductAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Загальна інформація");

            var analytic = await _analyticService.GetReceiptAnalyticAsync(
                filters.ReceiptFilters.BranchId,
                filters.ReceiptFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Кількість товарів у чеку";
            worksheet.Cell(1, 2).Value = "Середня кількість товарів у чеку";
            worksheet.Cell(1, 3).Value = "Середній чек";

            worksheet.Cell(2, 1).Value = analytic.Quantity;
            worksheet.Cell(2, 2).Value = analytic.AverageQuantity;
            worksheet.Cell(2, 3).Value = analytic.AverageAmount;
        }

        private async Task AddProductCategorySheetAsync(ProductAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Ходовий товар");

            var pagingDto = new PagingDTO
            {
                Skip = 0,
                Take = int.MaxValue,
            };

            var list = await _analyticService.GetProductCategoryAnalyticAsync(
                pagingDto,
                filters.ProductCategoryFilters.BranchId,
                filters.ProductCategoryFilters.CityId,
                filters.ProductCategoryFilters.StoreId,
                filters.ProductCategoryFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Категорія";
            worksheet.Cell(1, 2).Value = "Кількість";
            worksheet.Cell(1, 3).Value = "Чек";

            var index = 2;

            foreach (var item in list.Items)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Title);
                worksheet.Cell(index, 2).Value = CreateCell(item.Quantity);
                worksheet.Cell(index, 3).Value = CreateCell(item.Amount);

                index++;
            }
        }

        private async Task AddTradeTurnoverSheetAsync(ProductAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Товарообіг");

            var analytic = await _analyticService.GetTradeTurnoverAnalyticAsync(
                filters.TradeTurnoverFilters.BranchId,
                filters.TradeTurnoverFilters.CityId,
                filters.TradeTurnoverFilters.StoreId,
                filters.TradeTurnoverFilters.Date,
                filters.Age,
                filters.Sex,
                filters.TradeTurnoverFilters.Granularity
                );

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 3).Value = "Рік";
            worksheet.Cell(1, 4).Value = "Кількість";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Day);
                worksheet.Cell(index, 2).Value = CreateCell(item.Month.ToString());
                worksheet.Cell(index, 3).Value = CreateCell(item.Year);
                worksheet.Cell(index, 4).Value = CreateCell(item.Count);

                index++;
            }
        }

        private async Task AddPurchaseTimeSheetAsync(ProductAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Час здійснення покупки");

            var analytic = await _analyticService.GetPurchaseTimeAnalyticAsync(
                filters.PurchaseTimeFilters.BranchId,
                filters.PurchaseTimeFilters.CityId,
                filters.PurchaseTimeFilters.StoreId,
                filters.PurchaseTimeFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Година";
            worksheet.Cell(1, 2).Value = "Кількість";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Hour);
                worksheet.Cell(index, 2).Value = CreateCell(item.Count);

                index++;
            }
        }

        private async Task AddCardAndCouponUsageSheetAsync(ProductAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Використання карток і ваучерів");

            var analytic = await _analyticService.GetCardAndCouponUsageAnalyticByCategoryAsync(
                filters.CardAndCouponUsageAnalyticFilters.BranchId,
                filters.CardAndCouponUsageAnalyticFilters.CityId,
                filters.CardAndCouponUsageAnalyticFilters.StoreId,
                filters.CardAndCouponUsageAnalyticFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Категорія";
            worksheet.Cell(1, 2).Value = "Кількість ваучерів";
            worksheet.Cell(1, 3).Value = "Кількість карток";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.CategoryTitle);
                worksheet.Cell(index, 2).Value = CreateCell(item.CouponUsages);
                worksheet.Cell(index, 3).Value = CreateCell(item.CardUsages);

                index++;
            }
        }

        public async Task<ExportFile> GetLoyaltyAnalyticExportFileAsync(LoyaltyAnalyticExportFiltersDto filters)
        {
            using var workbook = new XLWorkbook();

            await AddAccumulationCardSheetAsync(filters, workbook);
            await AddProgramUsageSheetAsync(filters, workbook);
            await AddProgramSpentSheetAsync(filters, workbook);
            await AddCouponTimeUsageSheetAsync(filters, workbook);
            await AddCouponAnalyticSheetAsync(filters, workbook);
            await AddRfmCustomerCountSheetAsync(filters, workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"Програма-лояльності-{DateTimeOffset.UtcNow.Ticks}.xlsx"
            };
        }

        private async Task AddAccumulationCardSheetAsync(LoyaltyAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Активація картки +1");

            var pagingDto = new PagingDTO
            {
                Skip = 0,
                Take = int.MaxValue,
            };

            var list = await _analyticService.GetAccumulationCardAnalyticAsync(
                pagingDto,
                filters.AccumulationCardFilters.BranchId,
                filters.AccumulationCardFilters.CityId,
                filters.AccumulationCardFilters.StoreId,
                filters.AccumulationCardFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Назва";
            worksheet.Cell(1, 2).Value = "Видано";
            worksheet.Cell(1, 3).Value = "Активних";
            worksheet.Cell(1, 4).Value = "Архівований";
            worksheet.Cell(1, 5).Value = "Використаних";
            worksheet.Cell(1, 5).Value = "Неактивованих";
            worksheet.Cell(1, 6).Value = "Протермінованих";

            var index = 2;

            foreach (var item in list.Items)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Name);
                worksheet.Cell(index, 2).Value = CreateCell(item.DistributedCount);
                worksheet.Cell(index, 3).Value = CreateCell(item.ActiveCount);
                worksheet.Cell(index, 4).Value = CreateCell(item.ArchivedCount);
                worksheet.Cell(index, 5).Value = CreateCell(item.InactiveCount);
                worksheet.Cell(index, 6).Value = CreateCell(item.ExpiredCount);

                index++;
            }
        }

        private async Task AddProgramUsageSheetAsync(LoyaltyAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Частота використання програми");

            var analytic = await _analyticService.GetProgramUsageAnalyticAsync(
                filters.ProgramUsageFilters.BranchId,
                filters.ProgramUsageFilters.CityId,
                filters.ProgramUsageFilters.StoreId,
                filters.ProgramUsageFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 2).Value = "Рік";
            worksheet.Cell(1, 2).Value = "Кількість з карткою";
            worksheet.Cell(1, 2).Value = "Кількість без картки";

            var index = 2;
            var data = (from with in analytic.WithCardMetrics
                        join without in analytic.WithoutCardMetrics on with.Key equals without.Key
                        select new { with, without }
                        ).ToList();

            foreach (var item in data)
            {
                worksheet.Cell(1, 1).Value = CreateCell(item.with.Day);
                worksheet.Cell(1, 2).Value = CreateCell(item.with.Month.ToString());
                worksheet.Cell(1, 2).Value = CreateCell(item.with.Year);
                worksheet.Cell(1, 2).Value = CreateCell(item.with.Count);
                worksheet.Cell(1, 2).Value = CreateCell(item.without.Count);

                index++;
            }
        }

        private async Task AddProgramSpentSheetAsync(LoyaltyAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Витрати на програму");

            var analytic = await _analyticService.GetProgramSpentAnalyticAsync(
                filters.ProgramSpentFilters.BranchId,
                filters.ProgramSpentFilters.CityId,
                filters.ProgramSpentFilters.StoreId,
                filters.ProgramSpentFilters.Date,
                filters.Age,
                filters.Sex,
                filters.ProgramSpentFilters.Granularity);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 3).Value = "Рік";
            worksheet.Cell(1, 4).Value = "Витрати";

            var index = 2;

            foreach (var item in analytic.Metrics)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Day);
                worksheet.Cell(index, 2).Value = CreateCell(item.Month.ToString());
                worksheet.Cell(index, 3).Value = CreateCell(item.Year);
                worksheet.Cell(index, 4).Value = CreateCell(item.Amount);

                index++;
            }
        }

        private async Task AddCouponTimeUsageSheetAsync(LoyaltyAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Час на використання купона");

            var analytic = await _analyticService.GetCouponTimeUsageAnalyticAsync(
                filters.CouponTimeUsageFilters.BranchId,
                filters.CouponTimeUsageFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Мін. хв.";
            worksheet.Cell(1, 2).Value = "Макс. хв.";
            worksheet.Cell(1, 3).Value = "Сер. хв.";

            worksheet.Cell(2, 1).Value = CreateCell(analytic.MinMinutes);
            worksheet.Cell(2, 2).Value = CreateCell(analytic.MaxMinutes);
            worksheet.Cell(2, 3).Value = CreateCell(analytic.AverageMinutes);
        }

        private async Task AddCouponAnalyticSheetAsync(LoyaltyAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Емісія і використання ваучерів");

            var pagingDTO = new PagingDTO
            {
                Skip = 0,
                Take = int.MaxValue,
            };

            var list = await _analyticService.GetCouponAnalyticAsync(
                pagingDTO,
                filters.CouponAnalyticFilters.BranchId,
                filters.CouponAnalyticFilters.CityId,
                filters.CouponAnalyticFilters.StoreId,
                filters.CouponAnalyticFilters.Date,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Назва";
            worksheet.Cell(1, 2).Value = "Видано";
            worksheet.Cell(1, 3).Value = "Використано";

            var index = 2;

            foreach (var item in list.Items)
            {
                worksheet.Cell(index, 1).Value = CreateCell(item.Name);
                worksheet.Cell(index, 2).Value = CreateCell(item.DistributedCount);
                worksheet.Cell(index, 3).Value = CreateCell(item.UsedCount);
                index++;
            }
        }

        public async Task<ExportFile> GetFeedbackAnalyticExportFileAsync(FeedbackAnalyticExportFiltersDto filters)
        {
            using var workbook = new XLWorkbook();

            await AddFeedbackSheetAsync(filters, workbook);
            await AddTopicSheetAsync(filters, workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"Фідбеки-{DateTimeOffset.UtcNow.Ticks}.xlsx"
            };
        }

        private async Task AddFeedbackSheetAsync(FeedbackAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Фідбеки");
            var analytic = await _analyticService.GetFeedbackAnalytic(
                filters.FeedbackFilters.BranchId,
                filters.FeedbackFilters.Type,
                filters.FeedbackFilters.StoreId,
                filters.FeedbackFilters.Date,
                filters.ClientPlatform,
                filters.Age,
                filters.Sex);

            worksheet.Cell(1, 1).Value = "Всього";
            worksheet.Cell(1, 2).Value = "Середній час очікування";
            worksheet.Cell(1, 3).Value = "Середня оцінка";

            worksheet.Cell(2, 1).Value = CreateCell(analytic.Count);
            worksheet.Cell(2, 2).Value = CreateCell(analytic.AverageMinutesUntilAnswer);
            worksheet.Cell(2, 3).Value = CreateCell(analytic.AverageRating);
        }

        private async Task AddTopicSheetAsync(FeedbackAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Теми");
            var metrics = await _analyticService.GetTopicAnalyticAsync(
                filters.TopicFilters.BranchId,
                filters.TopicFilters.Date);

            worksheet.Cell(1, 1).Value = "Тема";
            worksheet.Cell(1, 2).Value = "Всього";
            worksheet.Cell(1, 3).Value = "Відсоток";

            var index = 2;

            foreach (var metric in metrics)
            {

                worksheet.Cell(index, 1).Value = CreateCell(metric.Title);
                worksheet.Cell(index, 2).Value = CreateCell(metric.Count);
                worksheet.Cell(index, 3).Value = CreateCell(metric.Percent);
                index++;
            }
        }

        private async Task AddRfmCustomerCountSheetAsync(LoyaltyAnalyticExportFiltersDto filters, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("RFM Сегменти Клієнтів");

            var analytic = await _analyticService.GetRfmCustomerCountAnalyticAsync(
                filters.RfmCustomerCountFilters.BranchId,
                filters.RfmCustomerCountFilters.CityId,
                filters.RfmCustomerCountFilters.StoreId,
                filters.RfmCustomerCountFilters.Date,
                filters.Age,
                filters.Sex,
                filters.RfmCustomerCountFilters.Granularity);

            worksheet.Cell(1, 1).Value = "День";
            worksheet.Cell(1, 2).Value = "Місяць";
            worksheet.Cell(1, 3).Value = "Рік";
            worksheet.Cell(1, 4).Value = "RFM Сегмент";
            worksheet.Cell(1, 5).Value = "Колір";
            worksheet.Cell(1, 6).Value = "Кількість клієнтів";

            var index = 2;

            foreach (var series in analytic.Series.OrderBy(s => s.RfmId))
            {
                foreach (var dataPoint in series.Data.OrderBy(d => d.Year).ThenBy(d => d.Month).ThenBy(d => d.Day))
                {
                    worksheet.Cell(index, 1).Value = CreateCell(dataPoint.Day);
                    worksheet.Cell(index, 2).Value = CreateCell(dataPoint.Month.ToString());
                    worksheet.Cell(index, 3).Value = CreateCell(dataPoint.Year);
                    worksheet.Cell(index, 4).Value = CreateCell(series.RfmName);
                    worksheet.Cell(index, 5).Value = CreateCell(series.RfmColor);
                    worksheet.Cell(index, 6).Value = CreateCell(dataPoint.CustomerCount);

                    index++;
                }
            }
        }

        private XLCellValue CreateCell(object? value)
        {
            if (value == null)
            {
                return Blank.Value;
            }

            var type = value.GetType();

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType != type)
            {
                value = Convert.ChangeType(value, underlyingType);
            }

            if (underlyingType == typeof(DateTime))
            {
                value = ((DateTime)value).FromUtcToTimezone(TimeZoneConstants.UATimezone);
            }

            if (underlyingType == typeof(Guid))
            {
                value = ((Guid)value).ToString();
            }

            return XLCellValue.FromObject(value);
        }
    }
}
