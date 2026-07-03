using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class StoreLogHistoryDTO
    {
        public int Id { get; set; }
        public string? Number { get; set; } = null;
        public string? Link { get; set; } = null;
        public string? Title { get; set; } = null;
        public string? Latitude { get; set; } = null;
        public string? Longitude { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? Address2 { get; set; } = null;
        public string? Region { get; set; } = null;
        public bool? IsActive { get; set; } = null;
        public int? StoreId { get; set; } = null;
        public int? CityId { get; set; } = null;
        public string ? MapLink { get; set; } = null;
    }
}