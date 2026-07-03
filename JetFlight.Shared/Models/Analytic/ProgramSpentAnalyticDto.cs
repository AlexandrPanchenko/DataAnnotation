using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class ProgramSpentAnalyticDto
    {
        public List<ProgramSpendAnalyticMetricDto> Metrics { get; set; }
        public ProgramSpendAnalyticMetricDto Max => Metrics.MaxBy(x => x.Amount);
        public ProgramSpendAnalyticMetricDto Min => Metrics.MinBy(x => x.Amount);
        public decimal TotalAmount => Metrics.Sum(x => x.Amount);
    }

    public class ProgramSpendAnalyticMetricDto
    {
        public Month Month { get; set; }
        public int Year { get; set; }
        public int? Day { get; set; }
        public DateTime Key => new DateTime(Year, (int)Month, Day ?? 1);
        public decimal Amount { get; set; }
    }
}
