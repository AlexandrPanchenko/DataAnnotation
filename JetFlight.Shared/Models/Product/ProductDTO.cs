namespace JetFlight.Shared.Models.Product
{
    public class ProductDTO
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public byte BranchId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string GroupTitle { get; set; }
        public string CategoryTitle { get; set; }
        public string BrandTitle { get; set; }
        public string ManufacturerTitle { get; set; }
        public List<SupplierDTO> Suppliers { get; set; }
        public List<StoreProductDTO> StoreProducts { get; set; }
    }

    public class StoreProductDTO
    {
        public int StoreId { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
    }
}
