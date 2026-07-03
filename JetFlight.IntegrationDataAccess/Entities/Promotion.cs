using JetFlight.Shared.Models;

namespace JetFlight.IntegrationDataAccess.Entities;
public class Promotion
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Offer { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; }
    public string? StoreCode { get; set; }

    /// <summary>
    /// Comma-separated airport IDs eligible for this loyalty offer (e.g. "15,25,35").
    /// </summary>
    public string? EligibleAirportIds { get; set; }

    public string? PromotionTypeId { get; set; }
    public string? ProductCode { get; set; }
    public decimal PromoPrice { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public bool InActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsComplexPromotion { get; set; }
    public PromotionType PromotionType { get; set; }
    public WebProductCategory WebProductCategory { get; set; }
    public Product Product { get; set; }
    public ItemUnit ItemUnit { get; set; }
    public ICollection<SavedPromotion> SavePromotions { get; set; }
}
