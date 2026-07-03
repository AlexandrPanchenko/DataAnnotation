namespace JetFlight.IntegrationDataAccess.Entities
{
    public class Product
    {
        public string Code { get; set; }
        public string FamilyCode { get; set; }
        public string BrandCode { get; set; }
        public string Title { get; set; }
        
        public string? OriginalFileName { get; set; }

        public byte[]? Image { get; set; }

        public bool? InActive { get; set; }

        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ProductFamily Family { get; set; }
        public ProductBrand Brand { get; set; }
        //public ICollection<ProductSupplier> Suppliers { get; set; }

        public ICollection<ReceiptProduct> ReceiptProducts { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}
