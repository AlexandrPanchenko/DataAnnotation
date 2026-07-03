
namespace JetFlight.Shared.Models.Promotion;

public class PromotionsCategoryDTO
{
    public string Code { get; set; }
    public string Title { get; set; }
    public string Icon { get; set; }
    public bool IsActive { get; set; }
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
