namespace JetFlight.Shared.Models.Analytic
{
    public class CustomerCountAnalyticDto
    {
        public List<CustomerCountMetricDto> Metrics { get; set; }
        public int DeletedCount => Metrics.Sum(x => x.DeletedCount);
        public int RegisteredCount => Metrics.Sum(x => x.RegisteredCount);
        public int ReturnedCount => Metrics.Sum(x => x.ReturnedCount);
    }
}
