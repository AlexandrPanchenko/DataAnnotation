using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Analytic
{
    public class CustomerCountMetricDto
    {
        public int? Day { get; set; }
        public Month Month { get; set; }
        public int Year { get; set; }
        public DateTime Key => new DateTime(Year, (int)Month, Day ?? 1);
        public int RegisteredCount { get; set; }
        public int DeletedCount { get; set; }
        public int ReturnedCount { get; set; }
    }
}
