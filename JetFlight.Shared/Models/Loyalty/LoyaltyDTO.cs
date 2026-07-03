using JetFlight.Shared.Models.Coupons;

namespace JetFlight.Shared.Models.Loyalty
{
    public class LoyaltyDto
    {
        public List<AssignedCustomerCouponDTO> CustomerCoupons { get; set; }
        public bool AutomaticWithdrawal { get; set; }
        public bool AccumulateRest { get; set; }
        public decimal Bonuses { get; set; }
    }
}
