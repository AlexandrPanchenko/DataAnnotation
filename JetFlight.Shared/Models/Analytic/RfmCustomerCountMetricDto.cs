using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class RfmCustomerCountMetricDto
    {
        public int RfmId { get; set; }
        public string RfmName { get; set; }
        public string RfmColor { get; set; }
        public int? Day { get; set; }
        public Month Month { get; set; }
        public int Year { get; set; }
        public DateTime Key => new DateTime(Year, (int)Month, Day ?? 1);
        public int CustomerCount { get; set; }
    }
}
