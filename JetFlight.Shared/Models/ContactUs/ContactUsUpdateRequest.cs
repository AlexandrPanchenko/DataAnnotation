using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsUpdateRequest
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("resolveMessage")]
        public string ResolveMessage { get; set; }

        [JsonPropertyName("resolveSignature")]
        public string ResolveSignature { get; set; }
    }
}
