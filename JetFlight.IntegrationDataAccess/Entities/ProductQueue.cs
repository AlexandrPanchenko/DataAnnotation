namespace JetFlight.IntegrationDataAccess.Entities;

public class ProductQueue
{
    public int Id { get; set; }

    public string Code { get; set; }

    public DateTime CreatedAt { get; set; }
        
    public DateTime? ProcessedAt { get; set; }

    public Product Product { get; set; }
}