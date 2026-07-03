using System.Text.Json;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Helpers
{
    public class DecimalTwoDecimalPlacesConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return decimal.Parse(reader.GetString()!);
            }
            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // Format with exactly 2 decimal places, including trailing zeros
            writer.WriteStringValue(value.ToString("F2"));
        }
    }
}

