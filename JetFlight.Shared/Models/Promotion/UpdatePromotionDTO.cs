using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Promotion
{
    public class UpdatePromotionDTO
    {
        public int? promotionTypeId { get; set; }
        public bool? isActive { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? offer { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public int? branchId { get; set; }
        public IFormFile? file { get; set; } = null;
        public List<string>? productCodes { get; set; }
    }
}
