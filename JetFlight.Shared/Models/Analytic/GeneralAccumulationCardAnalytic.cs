namespace JetFlight.Shared.Models.Analytic
{
    public class GeneralAccumulationCardAnalytic
    {
        public int DistributedCount { get; set; }
        public int ActiveCount { get; set; }
        public int ArchivedCount { get; set; }
        public int CompletedCount { get; set; }
        public int InactiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }
}
