namespace JetFlight.Shared.Models.Analytic
{
    public class CouponMetricDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistributedCount { get; set; }
        public int UsedCount { get; set; }
    }
}
