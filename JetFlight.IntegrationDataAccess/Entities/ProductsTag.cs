using System.ComponentModel.DataAnnotations;

namespace JetFlight.IntegrationDataAccess.Entities;

public class ProductsTag
{
    [Key]
    public  int Id { get; set; }
    public string Title { get; set; }
    public string Icon { get; set; }
    public bool IsActive { get; set; }

    public string Code { get; set; }

    public int? Position { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; }
}
