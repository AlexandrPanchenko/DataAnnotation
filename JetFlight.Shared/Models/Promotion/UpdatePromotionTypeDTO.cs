using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Promotion
{
    public class UpdatePromotionTypeDTO
    {
        public string? Title { get; set; }
        public bool? IsActive { get; set; }
        public int? position { get; set; }
        public DateTime? expiredAt { get; set; }
    }
}
