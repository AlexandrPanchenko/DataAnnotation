namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class SectionField : ISkipLogHistory
    {
        public int Id { get; set; }
        public int? SectionId { get; set; }
        public int? MediaFilesId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string RelatedTitle { get; set; }
        public int? Position { get; set; }
        public bool? Extendable { get; set; }
        public string? SubSectionTitle { get; set; }
        public string Placeholder { get; set; }
        public string? Dimensions { get; set; } = null;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Section Section { get; set; }
        public MediaFiles MediaFiles { get; set; }
    }
}
