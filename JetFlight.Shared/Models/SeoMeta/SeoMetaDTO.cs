using Microsoft.AspNetCore.Antiforgery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class SeoMetaDTO
    {
        [Required]
        public string EntityType { get; set; }
        [Required]
        public int EntityId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Keywords { get; set; } = null;
        public string? CanonicalUrl { get; set; } = null;
        public DateTime? UpdatedAt { get; set; }
    }
}
