using SPT.Common.Http;
using SPT.Common.Models.Logging;

namespace SPT.Common.Utils
{
    public static class ServerLog
    {
        public static void Custom(
            string source,
            string message,
            EServerLogTextColor color = EServerLogTextColor.White,
            EServerLogBackgroundColor backgroundColor = EServerLogBackgroundColor.Default)
        {
            Log(source, message, EServerLogLevel.Custom, color, backgroundColor);
        }

        public static void Error(string source, string message)
        {
            Log(source, message, EServerLogLevel.Error);
        }

        public static void Warn(string source, string message)
        {
            Log(source, message, EServerLogLevel.Warn);
        }

        public static void Success(string source, string message)
        {
            Log(source, message, EServerLogLevel.Success);
        }

        public static void Info(string source, string message)
        {
            Log(source, message, EServerLogLevel.Info);
        }

        public static void Debug(string source, string message)
        {
            Log(source, message, EServerLogLevel.Debug);
        }

        public static void Log(
            string source,
            string message,
            EServerLogLevel level = EServerLogLevel.Info,
            EServerLogTextColor color = EServerLogTextColor.White,
            EServerLogBackgroundColor backgroundColor = EServerLogBackgroundColor.Default)
        {
            ServerLogRequest request = new ServerLogRequest
            {
                Source = source,
                Message = message,
                Level = level,
                Color = color,
                BackgroundColor = backgroundColor
            };

            RequestHandler.PostJson("/singleplayer/log", Json.Serialize(request));
        }
    }
}
