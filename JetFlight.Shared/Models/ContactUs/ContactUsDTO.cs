using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsDTO
    {
        public int? id { get; set; }
        public int? customerId { get; set; } = null;
        [Required]
        public required string firstName { get; set; }
        public string? lastName { get; set; } = null;
        public string? email { get; set; } = null;
        public string? phoneNumber { get; set; } = null;
        [Required]
        public int topicId { get; set; }
        [Required]
        public required string message { get; set; }
        public DateTime? createdAt { get; set; } = null;
        public DateTime? updatedAt { get; set; } = null;
        public DateTime? processingDate { get; set; } = null;
        public int? assigneeId { get; set; } = null;
        public string? resolveMessage { get; set; } = null;
        public string? resolveSignature { get; set; } = null;

        public byte? branchId { get; set; } = null;

        public RequestStatus? status { get; set; } = null;

        public List<ContactUsFileDTO>? files { get; set; } = null;

    } 
}
