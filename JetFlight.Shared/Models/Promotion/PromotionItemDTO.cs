using JetFlight.Shared.Models.Product;

namespace JetFlight.Shared.Models.Promotion
{
    public class PromotionItemDTO
    {
        public int PromotionId { get; set; }
        public string ManufacturerCode { get; set; }
        public string ItemCategoryCode { get; set; }
        public string GroupCode { get; set; }
        public string BrandCode { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ProductDTO Product { get; set; }
    }
}
