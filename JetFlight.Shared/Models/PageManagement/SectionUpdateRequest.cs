using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement;

public class SectionUpdateRequest
{
    public int? id { get; set; } = null;
    public int? pageId { get; set; }
    public string name { get; set; }
    public string title { get; set; }
    public int? position { get; set; }
    public bool? isHtml { get; set; }
    [Required]
    public List<SectionFieldUpdateRequest> sectionFields { get; set; }


}