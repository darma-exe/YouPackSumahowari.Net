using System.Text.Json.Serialization;
using YouPackSumahowari.Net.JsonConverters;
using YouPackSumahowari.Net.Models.Enums;

namespace YouPackSumahowari.Net.Models;

public class Product
{
    [JsonPropertyName("productName")]
    public required string Name { get; set; }
    
    public required string Memo { get; set; }
    
    /// <summary>
    /// 受取人向けの配達予告通知が有効かどうかを示します。
    /// </summary>
    /// <remarks>
    /// <see cref="BoolToZeroOneStringConverter"/> を使用して、
    /// この真偽値をJSON出力時に0または1としてシリアル化します。
    /// </remarks>
    [JsonConverter(typeof(BoolToZeroOneStringConverter))]
    public required bool DeliveryNoticeMailService { get; set; }
    
    /// <summary>
    /// 受取人向けの不在持戻り通知が有効かどうかを示します。
    /// </summary>
    /// <remarks>
    /// <see cref="BoolToZeroOneStringConverter"/> を使用して、
    /// この真偽値をJSON出力時に0または1としてシリアル化します。
    /// </remarks>
    [JsonConverter(typeof(BoolToZeroOneStringConverter))]
    public required bool ReturnMailService { get; set; }
    
    /// <summary>
    /// 依頼主向けの配達完了通知が有効かどうかを示します。
    /// </summary>
    /// <remarks>
    /// <see cref="BoolToZeroOneStringConverter"/> を使用して、
    /// この真偽値をJSON出力時に0または1としてシリアル化します。
    /// </remarks>
    [JsonConverter(typeof(BoolToZeroOneStringConverter))]
    public required bool DeliveryCompletedMailService { get; set; }
    
    [JsonConverter(typeof(ProductSizeConverter))]
    public required int Size { get; set; }
    
    /// <summary>
    /// お届け希望時間帯
    /// </summary>
    [JsonConverter(typeof(IntToStringConverter<DeliveryTimeSlot>))]
    public required DeliveryTimeSlot ScheduledDeliveryTime { get; set; }
    
    /// <summary>
    /// 発送予定日
    /// </summary>
    [JsonConverter(typeof(DateOnlyConverter))]
    public required DateOnly ScheduledDate { get; set; }
    
    /// <summary>
    /// お届け希望日
    /// </summary>
    [JsonConverter(typeof(DateOnlyConverter))]
    public required DateOnly? ScheduledDeliveryDate { get; set; }

    [JsonConverter(typeof(EnumHashSetToStringHashSetConverter<Caution>))]
    public HashSet<Caution> Caution { get; set; } = [];
}
