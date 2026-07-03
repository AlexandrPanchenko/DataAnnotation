
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class WebProductCategory
    {
        [Key]
        public string Code { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        
        // Foreign key property
        public string? ParentCode { get; set; }
        
        // Navigation property
        [ForeignKey("ParentCode")]
        public WebProductCategory? Parent { get; set; }
    }
}
