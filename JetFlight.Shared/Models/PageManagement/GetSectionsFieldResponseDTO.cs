using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class GetSectionsFieldResponseDTO
    {
        public int Id { get; set; }
        public int? SectionId { get; set; }
        public int? OriginId { get; set; }
        public bool? IsDraft { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int? Position { get; set; }
        public bool? Extendable { get; set; }
        public string FilePath { get; set; }
        public string? SubSectionTitle { get; set; }
        public string? Dimensions { get; set; } = null;
        public string Placeholder { get; set; }
        [Required]
        public string Value { get; set; }
        public bool? IsHtml { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? RelatedTitle { get; set; }
    }
}
