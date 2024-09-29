using System.ComponentModel;

namespace YouPackSumahowari.Net.Models.Enums;

public enum DeliveryTimeSlot
{
    [Description("希望しない")]
    None = 0,

    [Description("午前中")]
    Morning = 60,

    [Description("12時頃〜14時頃")]
    Around12To14 = 62,

    [Description("14時頃〜16時頃")]
    Around14To16 = 63,

    [Description("16時頃〜18時頃")]
    Around16To18 = 64,

    [Description("18時頃〜20時頃")]
    Around18To20 = 65,

    [Description("19時頃〜21時頃")]
    Around19To21 = 67,

    [Description("20時頃〜21時頃")]
    Around20To21 = 66
}
