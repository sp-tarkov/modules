using System.Runtime.Serialization;

namespace Aki.Common.Models.Logging
{
    public enum EServerLogTextColor
    {
        [EnumMember(Value = "black")]
        Black ,
        [EnumMember(Value = "red")]
        Red,
        [EnumMember(Value = "green")]
        Green,
        [EnumMember(Value = "yellow")]
        Yellow,
        [EnumMember(Value = "blue")]
        Blue,
        [EnumMember(Value = "magenta")]
        Magenta,
        [EnumMember(Value = "cyan")]
        Cyan,
        [EnumMember(Value = "white")]
        White,
        [EnumMember(Value = "")]
        Gray
    }
}
