namespace JetFlight.Shared.Models.Analytic
{
    public class CardAndCouponUsageAnalyticByCategoryDTO
    {
        public List<CardAndCouponUsageMetricDTO> Metrics { get; set; }
        public int TotalCardUsages { get; set; }
        public int TotalCouponUsages { get; set; }
    }

    public class CardAndCouponUsageMetricDTO
    {
        public string CategoryCode { get; set; }
        public string CategoryTitle { get; set; }
        public int CardUsages { get; set; }
        public int CouponUsages { get; set; }
    }
}
