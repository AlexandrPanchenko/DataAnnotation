namespace JetFlight.Shared.Models.RFM
{
    public class BaseRfmDto
    {
        public string Color { get; set; }
        public RangeDTO<int> Period { get; set; }
        public RangeDTO<int> Amount { get; set; }
        public RangeDTO<int> Count { get; set; }
        public required string Name { get; set; }
    }
}
