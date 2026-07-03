using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class CountMetricDto
    {
        public int? Day { get; set; }
        public Month Month { get; set; }
        public int Year { get; set; }
        public DateTime Key => new DateTime(Year, (int)Month, Day ?? 1);
        public int Count { get; set; }
    }
}
