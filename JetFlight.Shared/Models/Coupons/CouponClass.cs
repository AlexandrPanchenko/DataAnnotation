using JetFlight.Shared.Extensions;

namespace JetFlight.Shared.Models.Coupons
{
    public enum CouponClass
    {
        [EnumDisplay("Персональний")]
        Personal,
        [EnumDisplay("Загальний")]
        Common,
    }
}
