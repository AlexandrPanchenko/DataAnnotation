using JetFlight.Shared.Models.ContactUs;

namespace JetFlight.Shared.Models.LogHistory
{
    public class ContactUsLogHistoryDTO
    {
        public int? CustomerId { get; set; }
        public string Message { get; set; }
        public int? AssigneeId { get; set; }
        public string ResolveMessage { get; set; }
        public RequestStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte? BranchId { get; set; }
    }
}
