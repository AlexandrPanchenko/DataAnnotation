using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class RfmCustomerCountAnalyticDto
    {
        public List<RfmSeriesDto> Series { get; set; }
    }

    public class RfmSeriesDto
    {
        public int RfmId { get; set; }
        public string RfmName { get; set; }
        public string RfmColor { get; set; }
        public List<RfmTimePointDto> Data { get; set; }
    }

    public class RfmTimePointDto
    {
        public DateTime Key => new DateTime(Year, (int)Month, Day ?? 1);
        public int? Day { get; set; }
        public Month Month { get; set; }
        public int Year { get; set; }
        public int CustomerCount { get; set; }
    }
}
