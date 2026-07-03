using JetFlight.Shared.Models.PageManagement;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Posts;

namespace JetFlight.Shared.Models.GlobalSearch;

public class GlobalSearchResult
{
    public List<PromotionSearchDTO> PromotionSearchResult { get; set; }
    public List<PageSearchDTO> PageSearchResult { get; set; }
    public List<PostSearchDTO> PostSearchResult { get; set; }
}
