namespace JetFlight.IntegrationDataAccess.Entities
{
    public class CustomerCoupon
    {
        public int Id { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public int UsedTimes { get; set; }
        public bool Activated { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
