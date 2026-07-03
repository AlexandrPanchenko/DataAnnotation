
namespace JetFlight.Shared.Models.Promotion;

public class SavedPromotionDTO
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int PromotionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public PromotionDTO Promotion { get; set; }
    public List<PromotionsTagDTO> PromotionsTag { get; set; }
}
