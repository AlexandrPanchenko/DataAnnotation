using JetFlight.Shared.Models.Product;

namespace JetFlight.Shared.Models.AccumulationCard
{
    public class CreateAccumulationCardDTO
    {
        public int CountToComplete { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string CouponDescription { get; set; }
        public List<string> ProductCodes { get; set; }
        public bool AllRequired { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<int> TargetIds { get; set; }
        public List<int> StoreIds { get; set; }
    }

    public class UpdateAccumulationCardDTO
    {
        public int Id { get; set; }
        public int CountToComplete { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string CouponDescription { get; set; }
        public List<string> ProductCodes { get; set; }
        public bool AllRequired { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<int> TargetIds { get; set; }
        public List<int> StoreIds { get; set; }
    }

    public class AdminAccumulationCardDTO
    {
        public int Id { get; set; }
        public int CountToComplete { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string CouponDescription { get; set; }
        public List<string> ProductCodes { get; set; }
        public bool AllRequired { get; set; }
        public AccumulationCardStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<int> TargetIds { get; set; }
        public List<int> StoreIds { get; set; }

        public List<ProductShortInfoDTO> Products { get; set; }
        public int ActivatedCount { get; set; }
        public int DistributedCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
