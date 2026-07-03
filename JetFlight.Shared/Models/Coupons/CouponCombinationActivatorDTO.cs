using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Coupons
{
    // Unified activator DTO hierarchy used by both CombinationPriceDiscount and BonusMultiplier
    [JsonDerivedType(typeof(CouponProductActivatorDTO), nameof(CouponProductActivatorDTO))]
    [JsonDerivedType(typeof(CouponCategoryActivatorDTO), nameof(CouponCategoryActivatorDTO))]
    [JsonDerivedType(typeof(CouponBrandActivatorDTO), nameof(CouponBrandActivatorDTO))]
    [JsonDerivedType(typeof(CouponSupplierActivatorDTO), nameof(CouponSupplierActivatorDTO))]
    [JsonDerivedType(typeof(CouponManufacturerActivatorDTO), nameof(CouponManufacturerActivatorDTO))]
    public abstract class CouponActivatorDTO
    {
    }

    public class CouponCategoryActivatorDTO : CouponActivatorDTO
    {
        public string CategoryCode { get; set; }
    }

    public class CouponBrandActivatorDTO : CouponActivatorDTO
    {
        public string BrandCode { get; set; }
    }

    public class CouponSupplierActivatorDTO : CouponActivatorDTO
    {
        public string SupplierCode { get; set; }
    }

    public class CouponManufacturerActivatorDTO : CouponActivatorDTO
    {
        public string ManufacturerCode { get; set; }
    }

    public class CouponProductActivatorDTO : CouponActivatorDTO
    {
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }
}
