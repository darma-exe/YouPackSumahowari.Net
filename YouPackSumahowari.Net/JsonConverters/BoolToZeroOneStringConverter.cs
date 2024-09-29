using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.JsonConverters;

public class BoolToZeroOneStringConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            return str == "1";
        }
        else
        {
            throw new JsonException("無効なトークンタイプです。'0' または '1' の文字列を期待しました。");
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        // trueなら"1", falseなら"0"を出力
        writer.WriteStringValue(value ? "1" : "0");
    }
}