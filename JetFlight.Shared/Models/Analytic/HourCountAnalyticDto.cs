namespace JetFlight.Shared.Models.Analytic
{
    public class HourCountAnalyticDto
    {
        public List<HourCountMetricDto> Metrics { get; set; }
        public HourCountMetricDto Max => Metrics.MaxBy(x => x.Count);
        public HourCountMetricDto Min => Metrics.MinBy(x => x.Count);
        public int TotalCount => Metrics.Sum(x => x.Count);
    }
}
