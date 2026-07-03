using JetFlight.Shared.Constants;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class SiteSettings
    {
        public int Id { get; set; }
        public byte BranchId { get; set; }
        public SiteSettingsKeys Key { get; set; }
        public string Value { get; set; }
    }
}
