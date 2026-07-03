using JetFlight.Shared.Models.Targets;
using JetFlight.Shared.Models.Users;

namespace JetFlight.Shared.Models.Analytic.Export
{
    public class GeneralAnalyticExportFiltersDto
    {
        public ClientPlatform? ClientPlatform { get; set; }
        public RangeDTO<int?> Age { get; set; } = new RangeDTO<int?>();
        public Sex? Sex { get; set; }
        public required GeneralCustomerFiltersDto GeneralCustomerFilters { get; set; }
        public required CustomerBirthdayFiltersDto CustomerBirthdayFilters { get; set; }
        public required CustomerDislocationFilterDto CustomerDislocationFilter { get; set; }
        public required ActiveCustomerFiltersDto ActiveCustomerFilters { get; set; }
        public required PageViewFiltersDto PageViewFilters { get; set; }
        public required WhereFindOutFilterDto WhereFindOutFilter { get; set; }
        public required TypeOfActivityFilterDto TypeOfActivityFilter { get; set; }
        public required NumberOfChildrenFilterDto NumberOfChildrenFilter { get; set; }
    }

    public class GeneralCustomerFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? ActiveStoreId { get; set; }
    }

    public class CustomerBirthdayFiltersDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? ActiveStoreId { get; set; }
        public Month? Month { get; set; }
    }

    public class CustomerDislocationFilterDto
    {
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
    }

    public class ActiveCustomerFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }

    public class PageViewFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }

    public class WhereFindOutFilterDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly?> Date { get; set; }
    }

    public class TypeOfActivityFilterDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly?> Date { get; set; }
    }

    public class NumberOfChildrenFilterDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly?> Date { get; set; }
    }
}
