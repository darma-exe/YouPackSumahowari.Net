using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.JsonConverters;

public class IntToStringConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Ensure the JSON token is a string
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a JSON string.");
        }

        var stringValue = reader.GetString();

        try
        {
            // Convert the string back to int
            if (int.TryParse(stringValue, out int intValue))
            {
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)intValue;
                }
                else if (typeof(T).IsEnum)
                {
                    return (T)Enum.ToObject(typeof(T), intValue);
                }
            }
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error converting '{stringValue}' to {typeof(T)}.", ex);
        }
        
        throw new JsonException($"Invalid value '{stringValue}' for type {typeof(T)}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        int intValue;

        if (typeof(T) == typeof(int))
        {
            intValue = (int)(object)value;
        }
        else if (typeof(T).IsEnum)
        {
            intValue = Convert.ToInt32(value);
        }
        else
        {
            throw new NotSupportedException($"Type {typeof(T)} is not supported by IntToStringConverter.");
        }

        writer.WriteStringValue(intValue.ToString());
    }
}