using Microsoft.AspNetCore.Http;

namespace JetFlight.Shared.Models.Promotion;
public class UpdatePromotionTagDTO
{
    public string? title { get; set; }
    public bool? isActive { get; set; }
    public IFormFile? file { get; set; } = null;
    public int? position { get; set; }
}
