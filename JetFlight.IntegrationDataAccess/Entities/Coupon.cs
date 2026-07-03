using JetFlight.Shared.Models.Coupons;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class Coupon : IAuditable, IRelatedAuditable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrivateName { get; set; }
        public string Description { get; set; }
        public string PrivateDescription { get; set; }
        public string Image { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }

        public EmissionBy? EmissionBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsCardCoupon { get; set; }

        public List<CouponToTarget> Targets { get; set; }
        public int UseTimes { get; set; }

        public int Emission { get; set; }

        public List<CouponToStore> StoreCodes { get; set; }
        
        public CouponClass Class { get; set; }

        public CouponStatus Status { get; set; }

        public CouponType Type { get; set; }

        public CouponCombinationPriceDiscount CouponCombinationPriceDiscount { get; set; }
        public CouponCombinationFixedPrice CouponCombinationFixedPrice { get; set; }
        public CouponAdditionalBonus CouponAdditionalBonus { get; set; }
        public CouponBonusMultiplier CouponBonusMultiplier { get; set; }
        public CouponDiscountPercent CouponDiscountPercent { get; set; }
        public CouponDiscountAmount CouponDiscountAmount { get; set; }
        public CouponProductFixedPrice CouponProductFixedPrice { get; set; }

        public List<CustomerCoupon> CustomerCoupons { get; set; }

        public List<Questionary> Questionaries { get; set; }

        public int? AccumulationCardId { get; set; }

        public AccumulationCard? AccumulationCard { get; set; }
    }


    public class CouponToTarget
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }
        public int TargetId { get; set; }
    }

    public class CouponToStore
    {
        public int Id { get; set; }
        public string StoreCode { get; set; }
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }
    }

    public class CouponCombinationProductActivator
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }

    public class CouponCombinationBrandActivator
    {
        public int Id { get; set; }
        public ProductBrand Brand { get; set; }
        public string BrandCode { get; set; }
    }

    public class CouponCombinationManufacturerActivator
    {
        public int Id { get; set; }
        public ProductManufacturer Manufacturer { get; set; }
        public string ManufacturerCode { get; set; }
    }

    public class CouponCombinationCategoryActivator
    {
        public int Id { get; set; }
        public ProductCategory Category { get; set; }
        public string CategoryCode { get; set; }
    }

    public class CouponCombinationSupplierActivator
    {
        public int Id { get; set; }
        public ProductsSupplier Supplier { get; set; }
        public string SupplierCode { get; set; }
    }

    public class CouponCombinationFixedPriceActivator
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }

    public class CouponCombinationPriceDiscount
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public bool AllRequired { get; set; }
        public List<CouponCombinationSupplierActivator> SupplierActivators { get; set; }
        public List<CouponCombinationProductActivator> ProductActivators { get; set; }
        public List<CouponCombinationCategoryActivator> CategoryActivators { get; set; }
        public List<CouponCombinationManufacturerActivator> ManufacturerActivators { get; set; }
        public List<CouponCombinationBrandActivator> BrandActivators { get; set; }
        public Product Product { get; set; }
        public string ProductCode { get; set; }
        public ProductsSupplier Supplier { get; set; }
        public string SupplierCode { get; set; }
        public decimal Price { get; set; }
        public decimal Compensation { get; set; }
        public int Quantity { get; set; }
    }
    public class CouponCombinationFixedPrice
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public List<CouponCombinationFixedPriceActivator> Activators { get; set; }
        public decimal FixedPrice { get; set; }
    }

    public class CouponAdditionalBonus
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public decimal Bonus { get; set; }
    }

    public class CouponBonusMultiplier 
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public float Multiplier { get; set; }
        //public Product Product { get; set; }
        //public string? ProductCode { get; set; }
        public List<CouponMultiplierSupplierActivator> SupplierActivators { get; set; }
        public List<CouponMultiplierProductActivator> ProductActivators { get; set; }
        public List<CouponMultiplierCategoryActivator> CategoryActivators { get; set; }
        public List<CouponMultiplierManufacturerActivator> ManufacturerActivators { get; set; }
        public List<CouponMultiplierBrandActivator> BrandActivators { get; set; }
    }

    // New activator entities for CouponBonusMultiplier
    public class CouponMultiplierProductActivator
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public CouponBonusMultiplier CouponBonusMultiplier { get; set; }
        public int CouponBonusMultiplierId { get; set; }
    }

    public class CouponMultiplierBrandActivator
    {
        public int Id { get; set; }
        public ProductBrand Brand { get; set; }
        public string BrandCode { get; set; }
        public CouponBonusMultiplier CouponBonusMultiplier { get; set; }
        public int CouponBonusMultiplierId { get; set; }
    }

    public class CouponMultiplierManufacturerActivator
    {
        public int Id { get; set; }
        public ProductManufacturer Manufacturer { get; set; }
        public string ManufacturerCode { get; set; }
        public CouponBonusMultiplier CouponBonusMultiplier { get; set; }
        public int CouponBonusMultiplierId { get; set; }
    }

    public class CouponMultiplierCategoryActivator
    {
        public int Id { get; set; }
        public ProductCategory Category { get; set; }
        public string CategoryCode { get; set; }
        public CouponBonusMultiplier CouponBonusMultiplier { get; set; }
        public int CouponBonusMultiplierId { get; set; }
    }

    public class CouponMultiplierSupplierActivator
    {
        public int Id { get; set; }
        public ProductsSupplier Supplier { get; set; }
        public string SupplierCode { get; set; }
        public CouponBonusMultiplier CouponBonusMultiplier { get; set; }
        public int CouponBonusMultiplierId { get; set; }
    }

    public class CouponDiscountPercent
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public float Percent { get; set; }
    }

    public class CouponDiscountAmount 
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CouponProductFixedPrice 
    {
        public int Id { get; set; }
        public Coupon Coupon { get; set; }
        public int CouponId { get; set; }
        public Product Product { get; set; }
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public int Quanitity { get; set; }
    }
}
