namespace JetFlight.Shared.Models.Store;

public class StoreUpdateResponse
{
    public StoreResponseDTO Item { get; set; }
    public List<string> Errors { get; set; }

    public StoreUpdateResponse()
    {
        Errors = new List<string>();
    }
}
