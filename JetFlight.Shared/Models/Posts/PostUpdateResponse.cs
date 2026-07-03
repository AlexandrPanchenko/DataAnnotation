namespace JetFlight.Shared.Models.Posts;

public class PostUpdateResponse
{
    public GetPostResponse Item { get; set; }
    public List<string> Errors { get; set; }

    public PostUpdateResponse()
    {
        Errors = new List<string>();
    }
}
