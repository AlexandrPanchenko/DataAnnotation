using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class CardAndCouponUsageAnalyticDTO
    {
        public int CardUsage { get; set; }
        public int CouponUsage { get; set; }
    }

    public class CardAndCouponUsageMetricDTo
    {
        public int Year { get; set; }
        public Month Month { get; set; }
        public int? Day { get; set; }

        public int CardUsage { get; set; }
        public int CouponUsage { get; set; }
    }
}
