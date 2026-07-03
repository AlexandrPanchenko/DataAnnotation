using JetFlight.Shared.Models.Product;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Coupons
{
    public class UsedCouponDTO
    {
        public int Id { get; set; }
        public CouponType Type { get; set; }
        public string Name { get; set; }
    
        public CouponRewardShortInfo Reward { get; set; }
    }

}
