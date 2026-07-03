using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement;

public class SectionFieldUpdateRequest
{
    public int id { get; set; }
    [Required]
    public int? sectionId { get; set; }
    public bool? isDraft { get; set; }
    public string key { get; set; }
    public string value { get; set; }
    public int? position { get; set; }
    public string type { get; set; }
    public bool? extendable { get; set; }
    public string title { get; set; }

    public string? dimensions { get; set; } = null;
    public string? placeholder { get; set; } = null;
    public bool? іsHtml { get; set; }
    public string? subSectionTitle { get; set; } = null;

    public string? relatedTitle { get; set; } = null;

    public IFormFile? file { get; set; } = null;

}