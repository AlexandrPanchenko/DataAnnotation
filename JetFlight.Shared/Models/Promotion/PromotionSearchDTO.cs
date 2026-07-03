namespace JetFlight.Shared.Models.Promotion;

public class PromotionSearchDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
    public decimal Price { get; set; }
    public decimal PromoPrice { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiredAt { get; set; }

    public byte BranchId { get; set; }
    public int StoreId { get; set; }

}
