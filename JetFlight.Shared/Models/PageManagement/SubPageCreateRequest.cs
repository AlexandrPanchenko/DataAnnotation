namespace JetFlight.Shared.Models.PageManagement
{
    public class SubPageCreateRequest
    {
        public string Name { get; set; }
        public RootPage RootPage { get; set; }
        public int BranchId { get; set; }
        public DateTime? ScheduledPublishDate { get; set; } = null;
        public string? Title { get; set; } = null;
    }
}
