using JetFlight.Shared.Models.ContactUs;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class ContactUs
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int? TopicId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public int? AssigneeId { get; set; }

        public string ResolveMessage { get; set; }
        public string ResolveSignature { get; set; }
        public RequestStatus Status { get; set; }

        public Topic Topic { get; set; }
        public byte? BranchId { get; set; }

        public ICollection<ContactUsAttachment> Attachments { get; set; }
    }
}
