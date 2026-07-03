using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class PageUpdateDTO
    {
        [Required]
        public int Id { get; set; }
        public string? Title { get; set; } = null;
        [Required]
        public string Name { get; set; }

        public DateTime? ScheduledPublishDate { get; set; } = null;
    }
}
