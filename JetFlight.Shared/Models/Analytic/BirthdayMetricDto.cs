using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class BirthdayMetricDto
    {
        public Month Month { get; set; }
        public int? Day { get; set; }
        public DateTime Key => new DateTime(4, (int)Month, Day ?? 1);
        public int Count { get; set; }
    }
}
