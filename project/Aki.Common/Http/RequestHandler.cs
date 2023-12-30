using System;
using System.Collections.Generic;
using System.Text;
using Aki.Common.Utils;
using BepInEx.Logging;

namespace Aki.Common.Http
{
    public static class RequestHandler
    {
        private static string _host;
        private static Request _request;
        private static Dictionary<string, string> _headers;
        private static ManualLogSource _logger;

        public static string SessionId { get; private set; }

        static RequestHandler()
        {
            _logger = Logger.CreateLogSource(nameof(RequestHandler));
            Initialize();
        }

        private static void Initialize()
        {
            _request = new Request();

            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                if (arg.Contains("BackendUrl"))
                {
                    string json = arg.Replace("-config=", string.Empty);
                    _host = Json.Deserialize<ServerConfig>(json).BackendUrl;
                }

                if (arg.Contains("-token="))
                {
                    SessionId = arg.Replace("-token=", string.Empty);
                    _headers = new Dictionary<string, string>()
                    {
                        { "Cookie", $"PHPSESSID={SessionId}" },
                        { "SessionId", SessionId }
                    };
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
            string url = (hasHost) ? path : _host + path;

            _logger.LogInfo($"Request GET data: {SessionId}:{url}");
            byte[] result = _request.Send(url, "GET", null, headers: _headers);

            ValidateData(result);
            return result;
        }

        public static string GetJson(string path, bool hasHost = false)
        {
            string url = (hasHost) ? path : _host + path;

            _logger.LogInfo($"Request GET json: {SessionId}:{url}");
            byte[] data = _request.Send(url, "GET", headers: _headers);
            string result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static string PostJson(string path, string json, bool hasHost = false)
        {
            string url = (hasHost) ? path : _host + path;

            _logger.LogInfo($"Request POST json: {SessionId}:{url}");
            byte[] data = _request.Send(url, "POST", Encoding.UTF8.GetBytes(json), true, "application/json", _headers);
            string result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static void PutJson(string path, string json, bool hasHost = false)
        {
            string url = (hasHost) ? path : _host + path;
            _logger.LogInfo($"Request PUT json: {SessionId}:{url}");
            _request.Send(url, "PUT", Encoding.UTF8.GetBytes(json), true, "application/json", _headers);
        }
    }
}
