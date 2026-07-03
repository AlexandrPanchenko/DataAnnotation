using Newtonsoft.Json;

namespace JetFlight.Shared;

public class SmsResponseModel
{
    [JsonProperty("id")]
    public string MessageId { get; set; }
}
