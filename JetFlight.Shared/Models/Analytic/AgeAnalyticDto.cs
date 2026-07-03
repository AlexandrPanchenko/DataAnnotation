namespace JetFlight.Shared.Models.Analytic
{
    public class AgeAnalyticDto
    {
        public int? MaxAge { get; set; }
        public int? MinAge { get; set; }
        public int? AverageAge { get; set; }
        public List<AgeMetricDto> Metrics { get; set; }
    }
}
