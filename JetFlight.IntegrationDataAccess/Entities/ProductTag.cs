
namespace JetFlight.IntegrationDataAccess.Entities;

public class ProductTag
{
    public int Id { get; set; }
    public int TagId { get; set; }
    public Product Product { get; set; }
    public ProductsTag ProductsTag { get; set; }
}
