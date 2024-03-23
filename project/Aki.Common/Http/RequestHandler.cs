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
        private static Client _httpClient;
        public static string Host { get; private set; }
        public static string SessionId { get; private set; }

        static RequestHandler()
        {
            _logger = Logger.CreateLogSource(nameof(RequestHandler));

            // lazy-load Host and SessionId
            Initialize();

            // initialize http client
            _httpClient = new Client(Host, SessionId)
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

            var result = _httpClient.Get(path);

            ValidateData(result);
            return result;
        }

        public static string GetJson(string path)
        {
            _logger.LogInfo($"Request GET json: {SessionId}:{Host}{path}");

            var data = _httpClient.Get(path);
            var result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static string PostJson(string path, string json)
        {
            _logger.LogInfo($"Request POST json: {SessionId}:{Host}{path}");

            var body = Encoding.UTF8.GetBytes(json);
            var data = _httpClient.Post(path, body);
            var result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static void PutJson(string path, string json)
        {
            _logger.LogInfo($"Request PUT json: {SessionId}:{Host}{path}");

            var body = Encoding.UTF8.GetBytes(json);
            _httpClient.Put(path, body);
        }
    }
}
