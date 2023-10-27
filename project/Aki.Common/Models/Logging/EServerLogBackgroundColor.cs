using System.Runtime.Serialization;

namespace Aki.Common.Models.Logging
{
    public enum EServerLogBackgroundColor
    {
        [EnumMember(Value = "")]
        Default,
        [EnumMember(Value = "blackBG")]
        Black,
        [EnumMember(Value = "redBG")]
        Red,
        [EnumMember(Value = "greenBG")]
        Green,
        [EnumMember(Value = "yellowBG")]
        Yellow,
        [EnumMember(Value = "blueBG")]
        Blue,
        [EnumMember(Value = "magentaBG")]
        Magenta,
        [EnumMember(Value = "cyanBG")]
        Cyan,
        [EnumMember(Value = "whiteBG")]
        White
    }
}
