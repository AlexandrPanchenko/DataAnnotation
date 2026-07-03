namespace JetFlight.Shared.Models.Analytic
{
    public class ProgramUsageAnalytic
    {
        public List<CountMetricDto> WithCardMetrics { get; set; }
        public List<CountMetricDto> WithoutCardMetrics { get; set; }

        public int TotalCountWithCard => WithCardMetrics.Sum(x => x.Count);
        public int TotalCountWithoutCard => WithoutCardMetrics.Sum(x => x.Count);

    }
}
