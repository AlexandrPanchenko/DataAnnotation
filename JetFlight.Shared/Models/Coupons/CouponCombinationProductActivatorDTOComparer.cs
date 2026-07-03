namespace JetFlight.Shared.Models.Coupons;

public class CouponProductActivatorDTOComparer : IEqualityComparer<CouponProductActivatorDTO>
{
    public bool Equals(CouponProductActivatorDTO? x, CouponProductActivatorDTO? y)
    {
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.ProductCode == y.ProductCode && x.Quantity == y.Quantity;
    }

    public int GetHashCode(CouponProductActivatorDTO obj)
    {
        return HashCode.Combine(obj.ProductCode, obj.Quantity);
    }
}