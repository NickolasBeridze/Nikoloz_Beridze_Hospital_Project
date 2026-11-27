using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Hospital_Project.Helpers
{
    internal static class JsonHelper
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Converters = { new FlexibleBooleanConverter() }
        };

        public static string Serialize<TData>(TData data)
            => JsonSerializer.Serialize(data, Options);

        public static TData? Deserialize<TData>(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<TData>(content, Options);
            }
            catch (JsonException)
            {
                string repaired = FixInvalidBooleanTokens(content);
                return JsonSerializer.Deserialize<TData>(repaired, Options);
            }
        }

        private static string FixInvalidBooleanTokens(string json)
        {
            json = Regex.Replace(json, @"(""IsPaid""\s*:\s*)T\b", "$1true");
            json = Regex.Replace(json, @"(""IsPaid""\s*:\s*)F\b", "$1false");

            json = Regex.Replace(json, @"(""IsPaid""\s*:\s*)""T""", "$1true");
            json = Regex.Replace(json, @"(""IsPaid""\s*:\s*)""F""", "$1false");

            return json;
        }
    }
    public class FlexibleBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    string? str = reader.GetString();
                    if (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(str, "t", StringComparison.OrdinalIgnoreCase) ||
                        str == "1")
                        return true;
                    if (string.Equals(str, "false", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(str, "f", StringComparison.OrdinalIgnoreCase) ||
                        str == "0")
                        return false;
                    throw new JsonException($"Cannot convert string '{str}' to boolean.");
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int num))
                        return num != 0;
                    throw new JsonException("Invalid number for boolean.");
                default:
                    throw new JsonException("Unexpected token for boolean.");
            }
        }
    
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
