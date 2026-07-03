using JetFlight.Shared.Models.Users;

namespace JetFlight.Shared.Models.Analytic.Export
{
    public class RegistrationAnalyticExportFiltersDto
    {
        public RangeDTO<int?> Age { get; set; } = new RangeDTO<int?>();
        public Sex? Sex { get; set; }
        public ClientPlatform? ClientPlatform { get; set; }
        public required RegistrationFiltersDto RegistrationFilters { get; set; }
        public required RegistrationHoursFiltersDto RegistrationHoursFilters { get; set; }
        public required CustomerCountFiltersDto CustomerCountFilters { get; set; }
    }

    public class RegistrationFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }

    public class RegistrationHoursFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
        public Granularity Granularity { get; set; }
    }

    public class CustomerCountFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }
}
