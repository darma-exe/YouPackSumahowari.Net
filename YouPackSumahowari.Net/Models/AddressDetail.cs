using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using YouPackSumahowari.Net.JsonConverters;
using YouPackSumahowari.Net.Models.Enums;

namespace YouPackSumahowari.Net.Models;

public class AddressDetail
{
    private string _postalCode = string.Empty;
    
    public required string Name { get; set; }
    
    [JsonPropertyName("postNo")]
    public required string PostalCode
    {
        get => _postalCode;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(PostalCode), "郵便番号は必須です。");

            var sanitized = value.Replace("-", "").Trim();

            if (!IsValidPostalCode(sanitized))
                throw new ArgumentException("無効な郵便番号の形式です。", nameof(PostalCode));

            _postalCode = sanitized;
        }
    }
    
    /// <summary>
    /// 市区町村
    /// </summary>
    public required string Address2 { get; set; }
    
    /// <summary>
    /// 大字
    /// </summary>
    public required string Address3 { get; set; }
    
    /// <summary>
    /// 丁目
    /// </summary>
    public required string Address4 { get; set; }
    
    public required string Address5 { get; set; }
    
    [JsonPropertyName("telNo")]
    public required string PhoneNumber { get; set; }
    
    [JsonPropertyName("mailAddress")]
    public required string Email { get; set; }
    
    [JsonConverter(typeof(TwoDigitPrefecturesCodeConverter))]
    public required PrefecturesCode PrefecturesCode { get; set; }
            
    /// <summary>
    /// 住所が未保存の場合はnullを設定し、JSONには反映しない。
    /// </summary>
    [JsonPropertyName("addressId")]
    public string? AddressID { get; set; }
            
    public required string DivCode { get; set; }
    
    static bool IsValidPostalCode(string postalCode)
    {
        return Regex.IsMatch(postalCode, @"^\d{7}$");
    }
}