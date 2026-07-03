using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class PageDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool? Published { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Link { get; set; }
        [Required]
        public int NumberOfSections { get; set; }
        public RootPage? RootPage { get; set; }

        public int? NumberOfPages { get; set; } = null;
        [Required]

        public bool IsActive { get; set; }

        [Required]

        public DateTime? PublishedAt { get; set; } = null;

        public DateTime? ScheduledPublishDate { get; set; } = null;

        [Required]

        public DateTime? UpdatedAt { get; set; } = null;
    }
}
