namespace JetFlight.Shared.Models.Shared;

public class PagingDTO
{
    public string OrderBy { get; set; }
    public OrderByDirectionTypes OrderByDirectionType { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

