using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Store;

public class StoreUpdateRequest
{
    [Required]
    public int id { get; set; }
    public string? number { get; set; } = null;
    public string? link { get; set; } = null;
    public string? title { get; set; } = null;
    [RegularExpression(RegexConstants.Latitude)]
    public string? latitude { get; set; } = null;
    [RegularExpression(RegexConstants.Longitude)]
    public string? longitude { get; set; } = null;
    public string? address { get; set; } = null;
    public string? address2 { get; set; } = null;
    public string? region { get; set; } = null;
    public bool? isActive { get; set; } = null;
    public int? storeId { get; set; } = null;
    public int? cityId { get; set; } = null;
    public IFormFile? file { get; set; } = null;
    [Required]
    public List<WorkingHoursDTO>? workingHours { get; set; } = new List<WorkingHoursDTO>();

    public Uri? mapLink { get; set; }
}