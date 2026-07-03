namespace JetFlight.Shared.Models.Promotion;

public class PromotionStoreDTO
{
    public int Id { get; set; }
    public string Store { get; set; }
    public int? BranchId { get; set; }
    public int PromotionId { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
}
