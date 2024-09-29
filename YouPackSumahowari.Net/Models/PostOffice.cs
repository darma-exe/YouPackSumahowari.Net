using System.Text.Json.Serialization;

namespace YouPackSumahowari.Net.Models;

public class PostOffice
{
    [JsonPropertyName("baseId")]
    public required string BaseID { get; set; }
    
    [JsonPropertyName("basePostNo")]
    public required string PostalCode { get; set; }
    
    [JsonPropertyName("baseAddress")]
    public required string Address { get; set; }
    
    [JsonPropertyName("baseTelNo")]
    public required string PhoneNumber { get; set; }
    
    [JsonPropertyName("baseName")]
    public required string Name { get; set; }
    
    public required string DivCode { get; set; }
    
    public required string DivName { get; set; }
    
    public required string StoreCode { get; set; }
}