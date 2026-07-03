using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class PageSearchDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

    }
}
