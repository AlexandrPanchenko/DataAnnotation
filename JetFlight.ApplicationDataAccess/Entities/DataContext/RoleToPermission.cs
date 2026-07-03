namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class RoleToPermission : ISkipLogHistory
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionsId { get; set; }

        public AdminRole Role { get; set; }
        public RolesPermission Permissions { get; set; }
    }
}
