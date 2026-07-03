namespace JetFlight.Shared.Models.Shared;

public class PagedListDTO<T>
{
    public IList<T> Items { get; set; }
    public int TotalItems { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }

    public PagedListDTO()
    {
        Items = new List<T>();
    }
}