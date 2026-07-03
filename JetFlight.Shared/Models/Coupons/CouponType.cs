using JetFlight.Shared.Extensions;

namespace JetFlight.Shared.Models.Coupons
{
    public enum CouponType
    {
        [EnumDisplay("Ціна товару (комбінований)")]
        CombinationPriceDiscount,
        [EnumDisplay("Ціна набору (комбінований)")]
        CombinationFixedPrice,
        [EnumDisplay("Екстра бонуси (фіксований)")]
        AdditionalBonus,
        [EnumDisplay("Екстра бонуси (множник)")]
        BonusMultiplier,
        [EnumDisplay("Знижка у чеку (%)")]
        DiscountPercent,
        [EnumDisplay("Знижка у чеку (грн.)")]
        DiscountAmount,
        [EnumDisplay("Ціна фіксована")]
        ProductFixedPrice,
    }
}
