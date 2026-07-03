namespace JetFlight.Shared.Models.ContactUs
{
    public class GetContactUsResponseDTO
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ResolveSignature { get; set; }
        public int? TopicId { get; set; }
        public string Message { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? AssigneeId { get; set; }
        public int? BranchId { get; set; }
        public DateTime? ProcessingDate { get; set; }

        public string ResolveMessage { get; set; }
        public RequestStatus Status { get; set; }
        public ICollection<ContactUsAttachmentsDTO> Files { get; set; } = null;
    }
}
