namespace JetFlight.IntegrationDataAccess.Entities
{
    public class ProductCategory
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string SegmentCode { get; set; }
        public ProductSegment Segment { get; set; }
        public ICollection<ProductFamily> Families { get; set; }
    }
}
