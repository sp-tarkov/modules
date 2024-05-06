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

        private static void ValidateData(string path, byte[] data)
        {
            if (data == null)
            {
                _logger.LogError($"[REQUEST FAILED] {path}");
            }

            _logger.LogInfo($"[REQUEST SUCCESSFUL] {path}");
        }

        private static void ValidateJson(string path, string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError($"[REQUEST FAILED] {path}");
            }

            _logger.LogInfo($"[REQUEST SUCCESSFUL] {path}");
        }

        public static async Task<byte[]> GetDataAsync(string path)
        {
            _logger.LogInfo($"[REQUEST]: {path}");
            
            var data = await HttpClient.GetAsync(path);

            ValidateData(path, data);
            return data;
        }

        public static byte[] GetData(string path)
        {
            return Task.Run(() => GetData(path)).Result;
        }

        public static async Task<string> GetJsonAsync(string path)
        {
            _logger.LogInfo($"[REQUEST]: {path}");
            
            var payload = await HttpClient.GetAsync(path);
            var body = Encoding.UTF8.GetString(payload);

            ValidateJson(path, body);
            return body;
        }

        public static string GetJson(string path)
        {
            return Task.Run(() => GetJsonAsync(path)).Result;
        }

        public static string PostJsonAsync(string path, string json)
        {
            _logger.LogInfo($"[REQUEST]: {path}");
            
            var payload = Encoding.UTF8.GetBytes(json);
            var data = HttpClient.Post(path, payload);
            var body = Encoding.UTF8.GetString(data);

            ValidateJson(path, body);
            return body;
        }

        public static string PostJson(string path, string json)
        {
            return Task.Run(() => PostJsonAsync(path, json)).Result;
        }

        // NOTE: returns status code
        public static async Task<string> PutJsonAsync(string path, string json)
        {
            _logger.LogInfo($"[REQUEST]: {path}");

            var payload = Encoding.UTF8.GetBytes(json);
            var data = await HttpClient.PutAsync(path, payload);
            var body = Encoding.UTF8.GetString(data);

            ValidateJson(path, body);
            return body;
        }

        // NOTE: returns status code
        public static string PutJson(string path, string json)
        {
            return Task.Run(() => PutJsonAsync(path, json)).Result;
        }

#region DEPRECATED, REMOVE IN 3.8.1
        [Obsolete("GetData(path, isHost) is deprecated, please use GetData(path) instead.")]
        public static byte[] GetData(string path, bool hasHost)
        {
            var url = (hasHost) ? path : Host + path;
            _logger.LogInfo($"Request GET data: {SessionId}:{url}");

            var headers = new Dictionary<string, string>()
            {
                { "Cookie", $"PHPSESSID={SessionId}" },
                { "SessionId", SessionId }
            };

            var request = new Request();
            var data = request.Send(url, "GET", null, headers: headers);

            ValidateData(url, data);
            return data;

        }

        [Obsolete("GetJson(path, isHost) is deprecated, please use GetJson(path) instead.")]
        public static string GetJson(string path, bool hasHost)
        {
            var url = (hasHost) ? path : Host + path;
            _logger.LogInfo($"Request GET json: {SessionId}:{url}");

            var headers = new Dictionary<string, string>()
            {
                { "Cookie", $"PHPSESSID={SessionId}" },
                { "SessionId", SessionId }
            };

            var request = new Request();
            var data = request.Send(url, "GET", headers: headers);
            var body = Encoding.UTF8.GetString(data);

            ValidateJson(url, body);
            return body;

        }

        [Obsolete("PostJson(path, json, isHost) is deprecated, please use PostJson(path, json) instead.")]
        public static string PostJson(string path, string json, bool hasHost)
        {
            var url = (hasHost) ? path : Host + path;
            _logger.LogInfo($"Request POST json: {SessionId}:{url}");

            var payload = Encoding.UTF8.GetBytes(json);
            var mime = WebConstants.Mime[".json"];
            var headers = new Dictionary<string, string>()
            {
                { "Cookie", $"PHPSESSID={SessionId}" },
                { "SessionId", SessionId }
            };

            var request = new Request();
            var data = request.Send(url, "POST", payload, true, mime, headers);
            var body = Encoding.UTF8.GetString(data);

            ValidateJson(url, body);
            return body;

        }

        [Obsolete("PutJson(path, json, isHost) is deprecated, please use PutJson(path, json) instead.")]
        public static void PutJson(string path, string json, bool hasHost)
        {
            var url = (hasHost) ? path : Host + path;
            _logger.LogInfo($"Request PUT json: {SessionId}:{url}");

            var payload = Encoding.UTF8.GetBytes(json);
            var mime = WebConstants.Mime[".json"];
            var headers = new Dictionary<string, string>()
            {
                { "Cookie", $"PHPSESSID={SessionId}" },
                { "SessionId", SessionId }
            };

            var request = new Request();
            request.Send(url, "PUT", payload, true, mime, headers);
        }
#endregion
    }
}
