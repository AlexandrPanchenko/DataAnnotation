namespace JetFlight.Shared.Models.Analytic
{
    public class DislocationMetricDto
    {
        public int StoreId { get; set; }
        public string StoreNumber { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public int CityId { get; set; }
        public int CustomerCount { get; set; }
        public int NumberOfVisits { get; set; }
    }
}
