// ReSharper disable IdentifierTypo
using System.ComponentModel;

namespace YouPackSumahowari.Net.Models.Enums;

public enum Caution
{
    [Description("こわれもの")]
    Kowaremono = 1,
    
    [Description("なまもの")]
    Namamono = 2,
    
    [Description("ビン類")]
    Binrui = 3,
    
    [Description("逆さま厳禁")]
    Sakasamagenkin = 4,
    
    [Description("下積み厳禁")]
    Shitazumigenkin = 5,
}