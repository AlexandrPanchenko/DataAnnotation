namespace JetFlight.IntegrationDataAccess.Entities;

public class SavedPromotion
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int PromotionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Customer Customer { get; set; }
    public Promotion Promotion { get; set; }
}
