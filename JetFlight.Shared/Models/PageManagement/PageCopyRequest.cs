using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement;

public class PageCopyRequest
{
    [Required]
    public int Id { get; set; }
}