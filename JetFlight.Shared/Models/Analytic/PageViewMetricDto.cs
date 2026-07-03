namespace JetFlight.Shared.Models.Analytic
{
    public class PageViewMetricDto
    {
        public string DisplayUrl { get; set; }
        public string Uri { get; set; }
        public string? Title { get; set; }
        public int Views { get; set; }
        public double VisitTime { get; set; }
    }
}
