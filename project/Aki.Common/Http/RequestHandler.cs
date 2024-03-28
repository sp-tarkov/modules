using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Aki.Common.Utils;

namespace Aki.Common.Http
{
    public static class RequestHandler
    {
        private static ManualLogSource _logger;
        public static readonly Client HttpClient;
        public static readonly string Host;
        public static readonly string SessionId;
        public static readonly bool IsLocal;

        static RequestHandler()
        {
            _logger = Logger.CreateLogSource(nameof(RequestHandler));
            
            // grab required info from command args
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

            IsLocal = Host.Contains("127.0.0.1")
                    || Host.Contains("localhost");

            // initialize http client
            HttpClient = new Client(Host, SessionId);
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

        public static byte[] GetData(string path)
        {
            _logger.LogInfo($"Request GET data: {SessionId}:{path}");
            
            var data = HttpClient.Get(path);

            ValidateData(data);
            return data;
        }

        public static string GetJson(string path)
        {
            _logger.LogInfo($"Request GET json: {SessionId}:{path}");
            
            var payload = HttpClient.Get(path);
            var body = Encoding.UTF8.GetString(payload);

            ValidateJson(body);
            return body;
        }

        public static string PostJson(string path, string json)
        {
            _logger.LogInfo($"Request POST json: {SessionId}:{path}");
            
            var payload = Encoding.UTF8.GetBytes(json);
            var data = HttpClient.Post(path, payload);
            var body = Encoding.UTF8.GetString(data);

            ValidateJson(body);
            return body;
        }

        public static void PutJson(string path, string json)
        {
            _logger.LogInfo($"Request PUT json: {SessionId}:{path}");

            var payload = Encoding.UTF8.GetBytes(json);
            HttpClient.Put(path, payload);
        }
    }
}
