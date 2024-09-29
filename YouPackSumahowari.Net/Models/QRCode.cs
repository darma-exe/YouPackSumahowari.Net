namespace YouPackSumahowari.Net.Models;

public class QRCode
{
    public required string TrackingNumber { get; init; }
    
    public required string AuthCode { get; init; }
    
    public required string QRCodeBase64 { get; init; }
    
    public required DateTime Limit { get; init; }
}