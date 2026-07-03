namespace JetFlight.Shared.Models.Analytic
{
    public class AccumulationCardMetricDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistributedCount { get; set; }
        public int ActiveCount {  get; set; }
        public int ArchivedCount {  get; set; }
        public int CompletedCount {  get; set; }
        public int InactiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }
}
