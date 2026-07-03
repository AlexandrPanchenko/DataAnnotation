namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class LogHistory : ISkipLogHistory
    {
        public int Id { get; set; }
        public int? AdminId { get; set; }
        public string EntityType { get; set; }
        public string? UpdatedFrom { get; set; }
        public string? UpdatedTo { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }  // Consider using an Enum type for actions
        public DateTime? Date { get; set; }

        public Admin Admin { get; set; }  // Navigation Property
    }
}
