using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.SendGrid;

public class CreateContactRequest
{
    public List<Contact> Contacts { get; set; }

    [JsonPropertyName("list_ids")]
    public List<Guid> ListIds { get; set; }
}

public class Contact
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    public string Email { get; set; }

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumber { get; set; }

    [JsonPropertyName("list_ids")]
    public List<Guid> ListIds { get; set; }
}
