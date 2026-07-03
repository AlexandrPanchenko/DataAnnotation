using Microsoft.AspNetCore.Http;

namespace JetFlight.Shared.Models.Promotion
{
    public class UpdatePromotionCategoryDTO
    {
        public string? title { get; set; }
        public IFormFile? file { get; set; } = null;
        public bool? isActive { get; set; }
        public int? position { get; set; }
    }
}
