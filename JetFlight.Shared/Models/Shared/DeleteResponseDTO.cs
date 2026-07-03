using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Shared;

public class DeleteResponseDTO
{
    [Required]
    public bool Result { get; set; }
    public List<string> Errors { get; set; }

    public DeleteResponseDTO()
    {
        Errors = new List<string>();
    }
}