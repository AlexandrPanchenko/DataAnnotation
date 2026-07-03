namespace JetFlight.Shared.Models.Coupons
{
    public class AdminCouponFilterDTO
    {
        public int? offset { get; set; }
        public int? limit { get; set; }
        public string? searchParam { get; set; }
        public byte? branchId { get; set; }
        public DateOnly? date { get; set; }
        public CouponClass? couponClass { get; set; }
        public CouponStatus? status { get; set; }
        public DateTime? startsEarlierThan { get; set; }
        public DateTime? expiresLaterThan { get; set; }
    }
}
