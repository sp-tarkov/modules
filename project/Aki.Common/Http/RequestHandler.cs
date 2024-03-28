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
        private static string _host;
        private static Client _request;
        private static ManualLogSource _logger;
        public static string SessionId { get; private set; }

        static RequestHandler()
        {
            _logger = Logger.CreateLogSource(nameof(RequestHandler));
            Initialize();
        }

        private static void Initialize()
        {
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
                }
            }

            _request = new Client(_host, SessionId);
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
            byte[] result = _request.Get(path);

            ValidateData(result);
            return result;
        }

        public static string GetJson(string path)
        {
            _logger.LogInfo($"Request GET json: {SessionId}:{path}");
            byte[] data = _request.Get(path);
            string result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static string PostJson(string path, string json)
        {
            _logger.LogInfo($"Request POST json: {SessionId}:{path}");
            byte[] data = _request.Post(path, Encoding.UTF8.GetBytes(json));
            string result = Encoding.UTF8.GetString(data);

            ValidateJson(result);
            return result;
        }

        public static void PutJson(string path, string json)
        {
            _logger.LogInfo($"Request PUT json: {SessionId}:{path}");
            _request.Put(path, Encoding.UTF8.GetBytes(json));
        }
    }
}
