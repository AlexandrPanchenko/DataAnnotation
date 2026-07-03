using JetFlight.Shared.Models.Users;

namespace JetFlight.Shared.Models.Analytic.Export
{
    public class ProductAnalyticExportFiltersDto
    {
        public RangeDTO<int?> Age { get; set; } = new RangeDTO<int?>();
        public Sex? Sex { get; set; }
        public required ReceiptFiltersDto ReceiptFilters { get; set; }
        public required ProductCategoryFiltersDto ProductCategoryFilters { get; set; }
        public required TradeTurnoverFiltersDto TradeTurnoverFilters { get; set; }
        public required PurchaseTimeFiltersDto PurchaseTimeFilters { get; set; }
        public required CardAndCouponUsageAnalyticFiltersDto CardAndCouponUsageAnalyticFilters { get; set; }
    }

    public class ReceiptFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }

    public class ProductCategoryFiltersDto
    {
        public byte? BranchId { get; set; }
        public byte? CityId { get; set; }
        public byte? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }

    public class TradeTurnoverFiltersDto
    {
        public byte? BranchId { get; set; }
        public byte? CityId { get; set; }
        public byte? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }

    public class PurchaseTimeFiltersDto
    {
        public byte? BranchId { get; set; }
        public byte? CityId { get; set; }
        public byte? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }
    
    public class CardAndCouponUsageAnalyticFiltersDto
    {
        public byte? BranchId { get; set; }
        public byte? CityId { get; set; }
        public byte? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }
}
