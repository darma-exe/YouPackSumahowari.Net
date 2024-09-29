![NuGet Version](https://img.shields.io/nuget/v/YouPackSumahowari.Net?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FYouPackSumahowari.Net%2F%23versions-body-tab)

## Usage
```cs
using YouPackSumahowari.Net;
using YouPackSumahowari.Net.Models;
using YouPackSumahowari.Net.Models.Enums;

var youPack = new SumahowariClient();

var postOffice = await youPack.FindPostOfficeAsync("東京都庁内郵便局");

var shipper = new AddressDetail
{
    Name = "東京 一郎",
    PostalCode = "1638001",
    PrefecturesCode = PrefecturesCode.Tokyo,
    Address2 = "新宿区",
    Address3 = "西新宿",
    Address4 = "2丁目",
    Address5 = "8−1",
    PhoneNumber = "0353211111",
    Email = "tokyo@example.com",
    DivCode = "FROM",
};
var consignee = new AddressDetail
{
    Name = "神奈川 次郎",
    PostalCode = "2318588",
    PrefecturesCode = PrefecturesCode.Kanagawa,
    Address2 = "横浜市",
    Address3 = "中区",
    Address4 = "日本大通",
    Address5 = "1",
    PhoneNumber = "0452101111",
    Email = "kanagawa@example.com",
    DivCode = "SENDTO",
};
var product = new Product
{
    Name = "書籍",
    Memo = "メモ",
    DeliveryNoticeMailService = false,
    ReturnMailService = false,
    DeliveryCompletedMailService = false,
    Size = 60,
    ScheduledDeliveryTime = DeliveryTimeSlot.Around14To16,
    ScheduledDate = new DateOnly(2024, 9, 29),
    ScheduledDeliveryDate = new DateOnly(2024, 10, 3),
    Caution = [Caution.Kowaremono, Caution.Binrui]
};
var label = new Label("NORMAL", shipper, consignee, product, postOffice);

await youPack.LoginAsync("abc@example.com", "password");

var (registerLabelResult, labelHeaderID, registerLabelErrorCode, registerLabelErrorMessage) = await youPack.RegisterLabelAsync(label);

var (registerPaymentResult, registerPaymentErrorCode, registerPaymentErrorMessage) = await youPack.RegisterLabelPaymentAsync(labelHeaderID, "1217");

var (base64QR, qrErrorCode, qrErrorMessage) = await youPack.GetLabelQRAsync(labelHeaderID);
```
