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

            // initialize http client
            HttpClient = new Client(Host, SessionId);
        }

        private static void Initialize()
        {
            
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
            byte[] result = HttpClient.Get(path);

            ValidateData(result);
            return result;
        }

        public static string GetJson(string path)
        {
            _logger.LogInfo($"Request GET json: {SessionId}:{path}");
            byte[] data = HttpClient.Get(path);
            string result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static string PostJson(string path, string json)
        {
            _logger.LogInfo($"Request POST json: {SessionId}:{path}");
            byte[] data = HttpClient.Post(path, Encoding.UTF8.GetBytes(json));
            string result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static void PutJson(string path, string json)
        {
            _logger.LogInfo($"Request PUT json: {SessionId}:{path}");
            HttpClient.Put(path, Encoding.UTF8.GetBytes(json));
        }
    }
}
