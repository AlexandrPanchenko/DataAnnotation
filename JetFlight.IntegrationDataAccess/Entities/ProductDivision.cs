namespace JetFlight.IntegrationDataAccess.Entities
{
    public class ProductDivision
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public ICollection<ProductSegment> Segments { get; set; }
    }
}
