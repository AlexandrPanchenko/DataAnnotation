using JetFlight.Shared.Models.Product;

namespace JetFlight.Shared.Models.AccumulationCard
{
    public class CustomerAccumulationCardDTO
    {
        public int Id { get; set; }
        public int CustomerAccumulationCardId { get; set; }
        public int Counter { get; set; }
        public int CountToComplete { get; set; }
        public CustomerAccumulationCardStatus Status { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string CouponDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<int> StoreIds { get; set; }
        public List<ProductShortInfoDTO> ActivationProducts { get; set; }
        public List<AccumulationCardReward> Rewards { get; set; }
    }

    public class CustomerAccumulationCardForAdminDTO
    {
        public int Id { get; set; }
        public int CustomerAccumulationCardId { get; set; }
        public int Counter { get; set; }
        public int CountToComplete { get; set; }
        public CustomerAccumulationCardStatusForAdmin Status { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string CouponDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<int> StoreIds { get; set; }
        public List<ProductShortInfoDTO> ActivationProducts { get; set; }
        public List<AccumulationCardReward> Rewards { get; set; }
    }

    public class AccumulationCardReward
    {
        public int CouponId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ProductShortInfoDTO Product { get; set; }
    }
}
