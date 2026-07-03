namespace JetFlight.Shared.Models.Posts;

public class PostCreateResponse
{
    public GetPostResponse Item { get; set; }
    public List<string> Errors { get; set; }

    public PostCreateResponse()
    {
        Errors = new List<string>();
    }
}
