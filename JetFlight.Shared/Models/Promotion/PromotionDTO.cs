using System.Text.Json.Serialization;
using JetFlight.Shared.Helpers;
using JetFlight.Shared.Models;

namespace JetFlight.Shared.Models.Promotion;

public class PromotionDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    [JsonConverter(typeof(DecimalTwoDecimalPlacesConverter))]
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    [JsonConverter(typeof(DecimalTwoDecimalPlacesConverter))]
    public decimal PromoPrice { get; set; }
    public string ItemUnit { get; set; }
    
    public DateTimeOffset StartedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string City { get; set; }
    public string PromotionsName { get;set; }
    public bool IsSaved { get; set; }
    public bool IsComplex { get; set; }
    public string? Offer { get; set; }

}
