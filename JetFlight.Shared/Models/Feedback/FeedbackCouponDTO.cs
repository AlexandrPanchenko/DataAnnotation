using JetFlight.Shared.Models.Coupons;

namespace JetFlight.Shared.Models.Feedback
{
    public class FeedbackCouponDTO
    {
        public int CustomerCouponId { get; set; }
        public int CouponId { get; set; }
        public string Name { get; set; }
        public string PrivateName { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime StartDate { get; set; }
        public CouponStatus Status { get; set; }
        public bool IsUsed { get; set; }
        public string Image { get; set; }
    }
}
