namespace JetFlight.Shared.Models.PageManagement;

public class SectionFieldDTO
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Title { get; set; }
    public string Key { get; set; }
    public int? MediaFilesId { get; set; }
    public string Type { get; set; }
    public string Placeholder { get; set; }
    public string SubSectionTitle { get; set; }
    public string Value { get; set; }
    public bool? Extendable { get; set; }
    public int? Position { get; set; }

    public string? RelatedTitle { get; set; }

    public string? Dimensions { get; set; } = null;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}
