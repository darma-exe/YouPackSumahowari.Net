using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.Models;

public class Label(
    string deliveryDivCode,
    AddressDetail shipper,
    AddressDetail consignee,
    Product product,
    PostOffice postOffice)
{
    [JsonPropertyName("delivDivCode")]
    public string DeliveryDivCode { get; set; } = deliveryDivCode;
    
    public AddressDetail Shipper { get; set; } = shipper;
    
    public AddressDetail Consignee { get; set; } = consignee;

    public Product Product { get; set; } = product;

    /// <summary>
    /// The originating post office
    /// </summary>
    public PostOffice Base { get; set; } = postOffice;
    
    public string TempFlag { get; set; } = "1";
    
    public string InputAddressFlag { get; set; } = "0";
    
    public string SelfSendFlag { get; set; } = "0";

    public string QrCodeFlag { get; set; } = "0";
}