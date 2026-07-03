namespace JetFlight.Shared.Models.Analytic
{
    public class BirthdayAnalyticDto
    {
        public BirthdayMetricDto Max { get; set; }
        public BirthdayMetricDto Min { get; set; }
        public List<BirthdayMetricDto> Metrics { get; set; }
    }
}
