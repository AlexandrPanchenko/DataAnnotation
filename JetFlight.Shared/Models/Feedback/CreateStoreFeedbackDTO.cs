using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Feedback
{
    public class CreateStoreFeedbackDTO
    {
        [Required]
        [Range(1, 5)]
        public byte rating { get; set; }

        public int storeId { get; set; }

        [Required]
        public string message { get; set; }

        public ICollection<FeedbackFileRequestDTO> Files { get; set; } = null;
    }
}
