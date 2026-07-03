
namespace JetFlight.IntegrationDataAccess.Entities;

public class PromotionType
{
    public int Id { get; set; }
    public string NavisionId { get; set; }
    public string Title { get; set; }
    public bool IsActive { get; set; }
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Promotion> Promotions { get; set; }
    public ICollection<PromotionTypeBranch> PromotionTypeBranches { get; set; }
}
