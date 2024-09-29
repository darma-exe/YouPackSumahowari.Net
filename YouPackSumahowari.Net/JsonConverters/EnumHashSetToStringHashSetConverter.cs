using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.JsonConverters
{
    public class EnumHashSetToStringHashSetConverter<T> : JsonConverter<HashSet<T>> where T : Enum
    {
        public override HashSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var hashSet = new HashSet<T>();

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException("Expected string token inside array");
                }

                var stringValue = reader.GetString();

                if (int.TryParse(stringValue, out int intValue))
                {
                    var enumValue = (T)Enum.ToObject(typeof(T), intValue);
                    hashSet.Add(enumValue);
                }
                else
                {
                    throw new JsonException($"Invalid integer value '{stringValue}' for enum type {typeof(T)}");
                }
            }

            return hashSet;
        }

        public override void Write(Utf8JsonWriter writer, HashSet<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var enumValue in value)
            {
                int intValue = Convert.ToInt32(enumValue);
                writer.WriteStringValue(intValue.ToString());
            }

            writer.WriteEndArray();
        }
    }
}