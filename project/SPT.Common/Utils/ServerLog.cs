using SPT.Common.Http;
using SPT.Common.Models.Logging;

namespace SPT.Common.Utils;

public static class ServerLog
{
    public static void Custom(
        string source,
        string message,
        EServerLogTextColor color = EServerLogTextColor.White,
        EServerLogBackgroundColor backgroundColor = EServerLogBackgroundColor.Default
    )
    {
        Log(source, message, EServerLogLevel.Information, color, backgroundColor);
    }

    public static void Error(string source, string message)
    {
        Log(source, message, EServerLogLevel.Error);
    }

    public static void Warn(string source, string message)
    {
        Log(source, message, EServerLogLevel.Warning);
    }

    public static void Success(string source, string message)
    {
        Log(source, message, EServerLogLevel.Information, EServerLogTextColor.Black, EServerLogBackgroundColor.Green);
    }

    public static void Info(string source, string message)
    {
        Log(source, message);
    }

    public static void Debug(string source, string message)
    {
        Log(source, message, EServerLogLevel.Debug);
    }

    public static void Log(
        string source,
        string message,
        EServerLogLevel level = EServerLogLevel.Information,
        EServerLogTextColor color = EServerLogTextColor.White,
        EServerLogBackgroundColor backgroundColor = EServerLogBackgroundColor.Default
    )
    {
        var request = new ServerLogRequest
        {
            Source = source,
            Message = message,
            Level = level,
            Color = color,
            BackgroundColor = backgroundColor,
        };

        RequestHandler.PostJson("/singleplayer/log", Json.Serialize(request));
    }
}
