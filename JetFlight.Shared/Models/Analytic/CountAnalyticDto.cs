namespace JetFlight.Shared.Models.Analytic
{
    public class CountAnalyticDto
    {
        public List<CountMetricDto> Metrics { get; set; }
        public CountMetricDto Max => Metrics.MaxBy(x => x.Count);
        public CountMetricDto Min => Metrics.MinBy(x => x.Count);
        public int TotalCount => Metrics.Sum(x => x.Count);
    }

}
