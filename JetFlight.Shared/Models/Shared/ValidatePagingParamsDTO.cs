namespace JetFlight.Shared.Models.Shared;

public class ValidatePagingParamsDTO
{
    public PagingDTO PagingDTO { get; set; }
    public List<string> Errors { get; set; }
    public ValidatePagingParamsDTO()
    {
        PagingDTO = new PagingDTO();
        Errors = new List<string>();
    }
}