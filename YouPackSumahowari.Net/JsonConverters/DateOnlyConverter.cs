using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.JsonConverters;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // トークンタイプが文字列であることを確認
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("期待されたJSONトークンは文字列です。");
        }

        // 文字列値を取得
        var dateString = reader.GetString();

        // "yyyyMMdd" フォーマットでパースを試みる
        if (!DateOnly.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            throw new JsonException($"無効な日付形式: {dateString}");
        }

        return date;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyyMMdd"));
    }
}