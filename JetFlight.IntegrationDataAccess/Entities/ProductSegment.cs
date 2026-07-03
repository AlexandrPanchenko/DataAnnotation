namespace JetFlight.IntegrationDataAccess.Entities
{
    public class ProductSegment
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string DivisionCode { get; set; }
        public ProductDivision Division { get; set; }
        public ICollection<ProductCategory> Categories { get; set; }
    }
}
