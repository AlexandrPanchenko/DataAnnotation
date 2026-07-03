namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Store
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Region { get; set; }
        public bool isActive { get; set; }
        public int CityId { get; set; }
        public MediaFiles MediaFile { get; set; }
        public byte? BranchId { get; set; }
        public City City { get; set; }

        public ICollection<WorkingHours> WorkingHours { get; set; }

        public Uri? MapLink { get; set; }
    }
}
