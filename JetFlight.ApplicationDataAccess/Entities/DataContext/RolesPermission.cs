namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class RolesPermission
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string Title { get; set; }
        public byte? Crud { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
