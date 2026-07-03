using System.Text.Json;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Helpers
{
    public class DateTimeOffsetConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Format as ISO 8601 with offset notation instead of "Z"
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff+00:00"));
        }
    }
}

