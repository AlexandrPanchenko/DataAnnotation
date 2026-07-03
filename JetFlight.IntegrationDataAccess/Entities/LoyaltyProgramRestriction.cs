namespace JetFlight.IntegrationDataAccess.Entities
{
    public class LoyaltyProgramRestriction
    {
        public int Id { get; set; }
        public string? DivisionCode { get; set; }
        public string? SegmentCode { get; set; }
        public string? CategoryCode { get; set; }
        public string? FamilyCode { get; set; }
        public string? ProductCode { get; set; }

        public bool ExcludeAllProducts { get; set; }
        public bool ExcludeMinPrice { get; set; }
        public bool ExcludeProductsWithPromotionPrice { get; set; }
        public bool ExcludePromotions { get; set; }
        public bool ExcludeOtherDocuments { get; set; }
        public bool ExcludePartialDiscount { get; set; }
        public bool ExcludeServiceCode { get; set; }

        public ProductDivision? Division { get; set; }
        public ProductSegment? Segment { get; set; }
        public ProductCategory? Category { get; set; }
        public ProductFamily? Family { get; set; }
        public Product? Product { get; set; }
    }
}
