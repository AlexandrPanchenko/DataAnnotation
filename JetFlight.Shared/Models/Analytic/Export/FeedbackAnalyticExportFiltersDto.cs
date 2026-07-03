using JetFlight.Shared.Models.Feedback;
using JetFlight.Shared.Models.Users;

namespace JetFlight.Shared.Models.Analytic.Export
{
    public class FeedbackAnalyticExportFiltersDto
    {
        public ClientPlatform? ClientPlatform { get; set; }
        public RangeDTO<int?> Age { get; set; } = new RangeDTO<int?>();
        public Sex? Sex { get; set; }
        public required FeedbackFiltersDto FeedbackFilters { get; set; }
        public required TopicFiltersDto TopicFilters { get; set; }
    }

    public class FeedbackFiltersDto
    {
        public byte? BranchId { get; set; }
        public FeedbackType? Type { get; set; }
        public int? StoreId { get; set; }
        public RangeDTO<DateOnly> Date { get; set; }
    }

    public class TopicFiltersDto
    {
        public byte? BranchId { get; set; }
        public RangeDTO<DateOnly?> Date { get; set; }
    }
}
