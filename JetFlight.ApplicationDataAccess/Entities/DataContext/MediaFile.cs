namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class MediaFiles
    {
        public int Id { get; set; }
        public string MimeType { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
