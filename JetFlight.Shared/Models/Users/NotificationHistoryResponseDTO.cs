using JetFlight.Shared.Models.Shared;

namespace JetFlight.Shared.Models.Users
{
    public class NotificationHistoryResponseDTO
    {
        public PagedListDTO<NotificationHistoryDTO> Notifications { get; set; }
        public int TotalRecords { get; set; }
        public int UnreadRecords { get; set; }
    }
}
