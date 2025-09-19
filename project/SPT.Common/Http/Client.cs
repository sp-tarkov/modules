using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using SPT.Common.Models;
using SPT.Common.Utils;
using UnityEngine.Networking;

namespace SPT.Common.Http;

public class Client(string address, string accountId, int retries = 3)
{
    private static ManualLogSource _logger = Logger.CreateLogSource(nameof(Client));

    public HttpClient HttpClient { get; } =
        new HttpClient(
            new HttpClientHandler
            {
                // set cookies in header instead
                UseCookies = false,

                // Bypass Cert validation in the httpServer - discard arguments as we dont use them
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            }
        );

    public HttpRequestMessage CreateNewHttpRequest(HttpMethod method, string path)
    {
        return new HttpRequestMessage()
        {
            Method = method,
            RequestUri = new Uri(address + path),
            Headers = { { "Cookie", $"PHPSESSID={accountId}" } },
        };
    }

    protected async Task<byte[]> SendAsync(HttpMethod method, string path, byte[] data, bool zipped = true)
    {
        using var request = CreateNewHttpRequest(method, path);

        if (data != null)
        {
            // Add payload to request
            if (zipped)
            {
                data = Zlib.Compress(data, ZlibCompression.Maximum);
            }

            request.Content = new ByteArrayContent(data);
        }

        // Send request
        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Http response status code: {response.StatusCode}");
        }

        var body = await response.Content.ReadAsByteArrayAsync();

        if (Zlib.IsCompressed(body))
        {
            body = Zlib.Decompress(body);
        }

        if (body == null)
        {
            // Payload doesn't contain data
            var code = response.StatusCode.ToString();
            body = Encoding.UTF8.GetBytes(code);
        }

        return body;
    }

    protected async Task<byte[]> SendWithRetriesAsync(HttpMethod method, string path, byte[] data, bool compress = true)
    {
        // NOTE: <= is intentional. 0 is send, 1/2/3 is retry
        for (var i = 0; i <= retries; i++)
        {
            try
            {
                return await SendAsync(method, path, data, compress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);

                if (i >= retries)
                {
                    throw;
                }
            }
        }

        return null;
    }

    public async Task DownloadAsync(string path, string filePath, Action<DownloadProgress> progressCallback = null)
    {
        var directoryPath = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using var request = UnityWebRequest.Get(address + path);
        request.downloadHandler = new DownloadHandlerFile(filePath) { removeFileOnAbort = true };
        request.certificateHandler = new FakeCertificateHandler();

        var operation = request.SendWebRequest();
        var startTime = DateTime.UtcNow;
        var lastUpdateTime = startTime;
        var lastBytesDownloaded = 0L;
        var totalBytes = 0L;

        while (!operation.isDone)
        {
            var currentTime = DateTime.UtcNow;
            var currentBytes = (long)request.downloadedBytes;

            if (totalBytes == 0 && request.GetResponseHeader("Content-Length") != null)
            {
                if (long.TryParse(request.GetResponseHeader("Content-Length"), out var contentLength))
                {
                    totalBytes = contentLength;
                }
            }

            var timeDiff = (currentTime - lastUpdateTime).TotalSeconds;
            var speed = 0.0;

            if (timeDiff >= 1)
            {
                var bytesDiff = currentBytes - lastBytesDownloaded;
                speed = bytesDiff / timeDiff;
                lastUpdateTime = currentTime;
                lastBytesDownloaded = currentBytes;
            }
            else
            {
                var totalTime = (currentTime - startTime).TotalSeconds;
                speed = totalTime > 0 ? currentBytes / totalTime : 0;
            }

            progressCallback?.Invoke(
                new DownloadProgress
                {
                    DownloadSpeed = DownloadProgress.FormatDownloadSpeed(speed),
                    FileSizeInfo = $"{DownloadProgress.FormatFileSize(currentBytes)} / {DownloadProgress.FormatFileSize(totalBytes)}",
                }
            );

            await Task.Delay(25);
        }
    }

    public async Task<byte[]> GetAsync(string path)
    {
        return await SendWithRetriesAsync(HttpMethod.Get, path, null);
    }

    public async Task<byte[]> PostAsync(string path, byte[] data, bool compress = true)
    {
        return await SendWithRetriesAsync(HttpMethod.Post, path, data, compress);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Returns status code as bytes</returns>
    public async Task<byte[]> PutAsync(string path, byte[] data, bool compress = true)
    {
        return await SendWithRetriesAsync(HttpMethod.Post, path, data, compress);
    }
}

public class DownloadProgress
{
    public string DownloadSpeed { get; set; }
    public string FileSizeInfo { get; set; }

    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:F1} KB";
        }

        return bytes < 1024 * 1024 * 1024 ? $"{bytes / (1024.0 * 1024):F1} MB" : $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }

    public static string FormatDownloadSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024)
        {
            return $"{bytesPerSecond:F0} B/s";
        }
        else if (bytesPerSecond < 1024 * 1024)
        {
            return $"{bytesPerSecond / 1024:F1} KB/s";
        }
        else
        {
            return bytesPerSecond < 1024 * 1024 * 1024
                ? $"{bytesPerSecond / (1024 * 1024):F1} MB/s"
                : $"{bytesPerSecond / (1024 * 1024 * 1024):F1} GB/s";
        }
    }
}
