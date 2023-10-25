using Aki.Common.Http;
using Aki.Common.Models.Logging;
using System;

namespace Aki.Common.Utils
{
    public static class ServerLog
    {
        public static void Custom(string source, string message, ServerLogTextColor color = null, ServerLogBackgroundColor backgroundColor = null)
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

        public static void Log(string source, string message, EServerLogLevel level = EServerLogLevel.Info, ServerLogTextColor color = null, ServerLogBackgroundColor backgroundColor= null)
        {
            ServerLogRequest request = new ServerLogRequest();
            request.Source = source;
            request.Message = message;
            request.Level = level;
            request.Color = color;
            request.BackgroundColor = backgroundColor;

            RequestHandler.PostJson("/singleplayer/log", Json.Serialize(request));
        }
    }
}
