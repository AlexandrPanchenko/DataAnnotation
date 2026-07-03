namespace JetFlight.Shared.Models.PageManagement
{
    public class SectionDTO
    {
        public int Id { get; set; }
        public bool? IsDraft { get; set; }
        public int? OriginId { get; set; }
        public int? PageId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int? Position { get; set; }
        public bool? IsActive { get; set; }
        public string Value { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public PageDTO Page { get; set; }
        public SectionDTO Origin { get; set; }
        public List<SectionFieldDTO> SectionFields { get; set; }
    }
}
