namespace JetFlight.IntegrationDataAccess.Entities
{
    public class ProductFamily
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string CategoryCode { get; set; }
        public ProductCategory Category { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
