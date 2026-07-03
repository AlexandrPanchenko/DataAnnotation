namespace JetFlight.Shared.Models.Analytic
{
    public class CouponAnalyticDto
    {
        public int UsedCount { get; set; }
        public int NotUsedCount { get; set; }
        public int Emission { get; set; }
        public int DistributedCount { get; set; }
    }
}
