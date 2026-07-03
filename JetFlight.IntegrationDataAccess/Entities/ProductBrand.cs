namespace JetFlight.IntegrationDataAccess.Entities
{
    public class ProductBrand
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string ManufacturerCode { get; set; }
        public ProductManufacturer Manufacturer { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
