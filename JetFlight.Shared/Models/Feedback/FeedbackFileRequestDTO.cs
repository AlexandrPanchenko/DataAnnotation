using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Feedback
{
    public class FeedbackFileRequestDTO
    {
        public string? name { get; set; }

        [Required]
        public IFormFile file { get; set; } = null;
    }
}
