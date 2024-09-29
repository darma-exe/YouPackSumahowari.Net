using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.JsonConverters;

public class ProductSizeConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // トークンの種類をチェック
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (int.TryParse(stringValue, out int value))
            {
                return value;
            }
            throw new JsonException($"文字列 \"{stringValue}\" を int に変換できません。");
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }
        else
        {
            throw new JsonException($"int のパース中に予期しないトークンタイプ {reader.TokenType} が見つかりました。");
        }
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        // 3桁に0埋めする
        writer.WriteStringValue(value.ToString("D3"));
    }
}