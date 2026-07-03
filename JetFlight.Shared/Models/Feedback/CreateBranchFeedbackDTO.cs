using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Feedback
{
    public class CreateBranchFeedbackDTO
    {
        [Required]
        [Range(1, 5)]
        public byte rating { get; set; }

        [Required]
        public string message { get; set; }
    }
}
