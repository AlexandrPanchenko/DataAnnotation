using JetFlight.Shared.Models.Users;

namespace JetFlight.Shared.Models.Analytic.Export
{
    public class LoyaltyAnalyticExportFiltersDto
    {
        public RangeDTO<int?> Age { get; set; } = new RangeDTO<int?>();
        public Sex? Sex { get; set; }
        public required AccumulationCardFiltersDto AccumulationCardFilters { get; set; }
        public required ProgramUsageFiltersDto ProgramUsageFilters { get; set; }
        public required ProgramSpentFiltersDto ProgramSpentFilters { get; set; }
        public required CouponTimeUsageFiltersDto CouponTimeUsageFilters { get; set; }
        public required CouponAnalyticFiltersDto CouponAnalyticFilters { get; set; }
        public required RfmCustomerCountFiltersDto RfmCustomerCountFilters { get; set; }
    }

    public class AccumulationCardFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }

    public class ProgramUsageFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }

    public class ProgramSpentFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }

    public class CouponTimeUsageFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }
    
    public class CouponAnalyticFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }

    public class RfmCustomerCountFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }
}
