namespace JetFlight.Shared.Models.Roles
{
    public class GetRoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Title { get; set; } = default!;
    }
}
