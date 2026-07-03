namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Section : ISkipLogHistory
    {
        public int Id { get; set; }
        public int? PageId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int? Position { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsHtml { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Page Page { get; set; }
        public Section Origin { get; set; }

        public ICollection<SectionField> SectionFields { get; set; } = new List<SectionField>();
    }
}
