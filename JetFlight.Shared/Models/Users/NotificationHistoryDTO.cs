
namespace JetFlight.Shared.Models.Users;
public class NotificationHistoryDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public bool IsRead { get; set; }
    public string MessageId { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
