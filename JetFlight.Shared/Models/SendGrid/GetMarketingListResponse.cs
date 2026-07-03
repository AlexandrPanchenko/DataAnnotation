namespace JetFlight.Shared.Models.SendGrid;

public class GetMarketingListResponse
{
    public List<MarketingList> Result { get; set; }
}

public class MarketingList
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
}