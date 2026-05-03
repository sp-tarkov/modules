using System;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using SPT.Common.Utils;

namespace SPT.Common.Http;

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

        IsLocal = Host.Contains("127.0.0.1") || Host.Contains("localhost");

        // initialize http client
        HttpClient = new Client(Host, SessionId);
    }

    private static void ValidateData(string path, byte[] data)
    {
        if (data == null)
        {
            _logger.LogError($"[REQUEST FAILED] {path}");
            return;
        }

        _logger.LogInfo($"[REQUEST SUCCESSFUL] {path}");
    }

    private static void ValidateJson(string path, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError($"[REQUEST FAILED] {path}");
            return;
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
        return Task.Run(() => GetDataAsync(path)).Result;
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

    public static async Task<string> PostJsonAsync(string path, string json)
    {
        _logger.LogInfo($"[REQUEST]: {path}");

        var payload = Encoding.UTF8.GetBytes(json);
        var data = await HttpClient.PostAsync(path, payload);
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
}
