using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class GetSectionsResponse : SectionDTO
    {
        [Required]
        public required List<GetSectionsFieldResponseDTO> SectionsFields { get; set; }

    }
}
