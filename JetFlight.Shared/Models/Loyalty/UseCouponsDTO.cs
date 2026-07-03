namespace JetFlight.Shared.Models.Loyalty
{
    public class UseCouponsDto
    {
        public string CardCode { get; set; }
        public HashSet<int> CustomerCouponIds { get; set; }
    }
}
