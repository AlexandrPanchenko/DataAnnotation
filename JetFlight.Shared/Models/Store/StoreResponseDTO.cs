using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Store
{
    public class StoreResponseDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        [Required]
        public string City { get; set; }

        [Required]
        public int? CityId { get; set; }
        [Required]
        public int? BranchId { get; set; }
        public string? Address { get; set; }
        public string? Address2 { get; set; }
        public string? Region { get; set; }
        public string? StoreCode { get; set; }

        public string? ImagePath { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]

        public List<WorkingHoursDTO> WorkingHours { get; set; }

        public Uri? MapLink { get; set; }

    }
}
