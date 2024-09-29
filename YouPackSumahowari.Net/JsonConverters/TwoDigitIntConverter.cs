using System.Text.Json;
using System.Text.Json.Serialization;
using YouPackSumahowari.Net.Models.Enums;

namespace YouPackSumahowari.Net.JsonConverters;

public class TwoDigitPrefecturesCodeConverter : JsonConverter<PrefecturesCode>
{
    public override PrefecturesCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int value;

        // JSONのトークンが文字列の場合
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (int.TryParse(stringValue, out value))
            {
                return ConvertToPrefecturesCode(value);
            }
            throw new JsonException($"\"{stringValue}\" は有効な都道府県コードではありません。");
        }
        // JSONのトークンが数値の場合
        else if (reader.TokenType == JsonTokenType.Number)
        {
            value = reader.GetInt32();
            return ConvertToPrefecturesCode(value);
        }

        throw new JsonException($"都道府県コードのパース中に予期しないトークンが見つかりました。: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, PrefecturesCode value, JsonSerializerOptions options)
    {
        int intValue = (int)value;

        // 整数を2桁の文字列にフォーマット（必要に応じて先頭に0を追加）
        string formatted = intValue.ToString("D2");
        writer.WriteStringValue(formatted);
    }

    /// <summary>
    /// 整数値を PrefecturesCode 列挙型に変換します。
    /// 有効な値でない場合は例外をスローします。
    /// </summary>
    /// <param name="value">整数値</param>
    /// <returns>対応する PrefecturesCode 列挙値</returns>
    private PrefecturesCode ConvertToPrefecturesCode(int value)
    {
        if (Enum.IsDefined(typeof(PrefecturesCode), value))
        {
            return (PrefecturesCode)value;
        }
        throw new JsonException($"値 {value} は有効な都道府県コードではありません。");
    }
}