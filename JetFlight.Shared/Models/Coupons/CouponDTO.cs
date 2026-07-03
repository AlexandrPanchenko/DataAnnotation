using JetFlight.Shared.Models.Product;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Coupons
{
    public class CreateCouponDTO
    {
        public string Name { get; set; }
        public string PrivateName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public EmissionBy? EmissionBy { get; set; }
        public string Image { get; set; }
        public List<int> StoreIds { get; set; }
        public CouponDetailsDTO Details { get; set; }
        public string PrivateDescription { get; set; }
        public CouponClass Class { get; set; }
        public List<int> TargetIds { get; set; }
        public int UseTimes { get; set; }
        public int Emission { get; set; }
    }

    public class UpdateCouponDTO
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string PrivateName { get; set; }
        public string Description { get; set; }
        public string PrivateDescription { get; set; }
        public CouponClass Class { get; set; }
        public List<int> TargetIds { get; set; }
        public int UseTimes { get; set; }
        public int Emission { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public EmissionBy? EmissionBy { get; set; }
        public List<int> StoreIds { get; set; }
        public CouponDetailsDTO Details { get; set; }
    }

    public class AssignedCustomerCouponDTO
    {
        public int Id { get; set; }
        public CouponType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Image { get; set; }
        public List<int> StoreIds { get; set; }
        public CouponRewardShortInfo Reward { get; set; }
        public int CustomerCouponId { get; set; }
        public int RemainingTimes { get; set; }
        public bool Activated { get; set; }
    }

    public class CustomerCouponForAdminDTO
    {
        public int Id { get; set; }
        public CouponType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Image { get; set; }
        public List<int> StoreIds { get; set; }
        public CouponRewardShortInfo Reward { get; set; }
        public CustomerCouponStatus Status { get; set; }
        public int CustomerCouponId { get; set; }
        public int RemainingTimes { get; set; }
        public bool Activated { get; set; }
    }

    public class CouponRewardShortInfo 
    {
        public string Value { get; set; }
        public ProductShortInfoDTO Product { get; set; }
        public int? Quantity { get; set; }
    }

    public class AdminCouponDTO
    {
        public int Id { get; set; }
        public CouponType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public CouponStatus Status { get; set; }
        public CouponClass Class { get; set; }

        public string Name { get; set; }
        public string PrivateName { get; set; }
        public string Description { get; set; }
        public string? PrivateDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public EmissionBy? EmissionBy { get; set; }
        public List<int> TargetIds { get; set; }
        public int? UseTimes { get; set; }
        public string Image { get; set; }
        public List<int> StoreIds { get; set; }
        public CouponDetailsDTO Details { get; set; }
        public int DistributedCount { get; set; }
        public int UsedCount { get; set; }
        public int Emission {  get; set; }
        public int? AvailableCount { get; set; }
        public double? UsedPercentage => CalculatePercentage();

        public CouponRewardShortInfo Reward { get; set; }

        private double? CalculatePercentage()
        {
            if (Status == CouponStatus.Inactive)
            {
                return null;
            }

            if (Class == CouponClass.Common)
            {
                return DistributedCount == 0 ? 100 : ((double)UsedCount) / DistributedCount * 100;
            }
            else
            {
                return (Emission == 0 ? 100 : ((double)UsedCount) / Emission) * 100;
            }
        }
    }

    [JsonDerivedType(typeof(CouponCombinationPriceDiscountDTO), nameof(CouponCombinationPriceDiscountDTO))]
    [JsonDerivedType(typeof(CouponCombinationFixedPriceDTO), nameof(CouponCombinationFixedPriceDTO))]
    [JsonDerivedType(typeof(CouponAdditionalBonusDTO), nameof(CouponAdditionalBonusDTO))]
    [JsonDerivedType(typeof(CouponBonusMultiplierDTO), nameof(CouponBonusMultiplierDTO))]
    [JsonDerivedType(typeof(CouponDiscountPercentDTO), nameof(CouponDiscountPercentDTO))]
    [JsonDerivedType(typeof(CouponDiscountAmountDTO), nameof(CouponDiscountAmountDTO))]
    [JsonDerivedType(typeof(CouponProductFixedPriceDTO), nameof(CouponProductFixedPriceDTO))]
    public abstract class CouponDetailsDTO
    {

    }

    public class CouponCombinationPriceDiscountDTO : CouponDetailsDTO
    {
        public bool AllRequired { get; set; }
        public List<CouponActivatorDTO> Activators { get; set; } = new();
        public string ProductCode { get; set; }
        public string SupplierCode { get; set; }
        public decimal Price { get; set; }
        public decimal Compensation { get; set; }
        public int Quantity { get; set; }
    }

    public class CouponCombinationFixedPriceDTO : CouponDetailsDTO
    {
        public List<CouponProductActivatorDTO> Activators { get; set; } = new();
        public decimal FixedPrice { get; set; }
    }

    public class CouponAdditionalBonusDTO : CouponDetailsDTO
    {
        public decimal Bonus { get; set; }
    }

    public class CouponBonusMultiplierDTO : CouponDetailsDTO
    {
        public float Multiplier { get; set; }
        public List<CouponActivatorDTO> Activators { get; set; } = new();
    }

    public class CouponDiscountPercentDTO : CouponDetailsDTO
    {
        public float Percent { get; set; }
    }

    public class CouponDiscountAmountDTO : CouponDetailsDTO
    {
        public decimal Amount { get; set; }
    }

    public class CouponProductFixedPriceDTO : CouponDetailsDTO
    {
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public int Quanitity { get; set; }
    }
}
