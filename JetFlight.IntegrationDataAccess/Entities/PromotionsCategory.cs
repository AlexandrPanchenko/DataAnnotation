
namespace JetFlight.IntegrationDataAccess.Entities;
public class PromotionCategory
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ProductCategory Category { get; set; }
    public string CategoryCode { get; set; }
}
