using System;
using System.Collections.Generic;
using System.Text;
using Aki.Common.Utils;
using BepInEx.Logging;

namespace Aki.Common.Http
{
    public static class RequestHandler
    {
        private static ManualLogSource _logger;
        public static string Host { get; private set; }
        public static string SessionId { get; private set; }

        static RequestHandler()
        {
            _logger = Logger.CreateLogSource(nameof(RequestHandler));
            Initialize();
        }

        private static void Initialize()
        {
            var args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                if (arg.Contains("BackendUrl"))
                {
                    var json = arg.Replace("-config=", string.Empty);
                    Host = Json.Deserialize<ServerConfig>(json).BackendUrl;
                }

                if (arg.Contains("-token="))
                {
                    SessionId = arg.Replace("-token=", string.Empty);
                }
            }
        }

        private static void ValidateData(byte[] data)
        {
            if (data == null)
            {
                _logger.LogError($"Request failed, body is null");
            }

            _logger.LogInfo($"Request was successful");
        }

        private static void ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError($"Request failed, body is null");
            }

            _logger.LogInfo($"Request was successful");
        }

        public static byte[] GetData(string path, bool hasHost = false)
        {
            var url = (hasHost) ? path : Host + path;
            _logger.LogInfo($"Request GET data: {SessionId}:{url}");
            
            using (var client = new Client(Host, SessionId))
            {
                var result = client.Get(path);

                ValidateData(result);
                return result;
            }
        }

        public static string GetJson(string path)
        {
            _logger.LogInfo($"Request GET json: {SessionId}:{Host}{path}");

            using (var client = new Client(Host, SessionId))
            {
                var data = client.Get(path);
                var result = Encoding.UTF8.GetString(data);

                ValidateJson(result);
                return result;
            }
        }

        public static string PostJson(string path, string json)
        {
            _logger.LogInfo($"Request POST json: {SessionId}:{Host}{path}");

            using (var client = new Client(Host, SessionId))
            {
                var body = Encoding.UTF8.GetBytes(json);
                var data = client.Post(path, body);
                var result = Encoding.UTF8.GetString(data);

                ValidateJson(result);
                return result;
            }
        }

        public static void PutJson(string path, string json)
        {
            _logger.LogInfo($"Request PUT json: {SessionId}:{Host}{path}");

            using (var client = new Client(Host, SessionId))
            {
                var body = Encoding.UTF8.GetBytes(json);
                client.Put(path, body);
            }
        }
    }
}
