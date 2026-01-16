using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;

namespace SPT.Custom.Utils;

public static class BundleManager
{
    private const string CachePath = "SPT/user/cache/bundles/";
    private static readonly ManualLogSource _logger;
    public static readonly ConcurrentDictionary<string, BundleItem> Bundles;

    static BundleManager()
    {
        _logger = Logger.CreateLogSource(nameof(BundleManager));
        Bundles = new ConcurrentDictionary<string, BundleItem>();
    }

    public static string GetBundlePath(BundleItem bundle)
    {
        return RequestHandler.IsLocal ? $"SPT/{bundle.ModPath}/bundles/" : CachePath;
    }

    public static string GetBundleFilePath(BundleItem bundle)
    {
        return GetBundlePath(bundle) + bundle.FileName;
    }

    public static async Task DownloadManifest()
    {
        // get bundles
        var json = await RequestHandler.GetJsonAsync("/singleplayer/bundles");
        var bundles = JsonConvert.DeserializeObject<BundleItem[]>(json);

        foreach (var bundle in bundles)
        {
            Bundles.TryAdd(bundle.FileName, bundle);
        }
    }

    public static async Task DownloadBundle(BundleItem bundle, System.Action<DownloadProgress> progressCallback)
    {
        var filepath = GetBundleFilePath(bundle);
        await RequestHandler.HttpClient.DownloadAsync($"/files/bundle/{bundle.FileName}", filepath, progressCallback);
    }

    // Handles both the check for initially acquiring and also re-acquiring a file.
    /*
    public static async Task<bool> ShouldAcquire(BundleItem bundle)
    {
        // If this is a local bundle, we never want to re-acquire it, otherwise we risk deleting it from the server
        if (RequestHandler.IsLocal)
        {
            _logger.LogInfo($"MOD: Loading locally {bundle.FileName}");
            return false;
        }

        // read cache
        var filepath = GetBundleFilePath(bundle);

        if (VFS.Exists(filepath))
        {
            // calculate hash
            var data = await VFS.ReadFileAsync(filepath);
            var crc = Crc32.HashToUInt32(data);

            if (crc == bundle.Crc)
            {
                // file is up-to-date
                _logger.LogInfo($"CACHE: Loading locally {bundle.FileName}");
                return false;
            }
            else
            {
                // crc doesn't match, reaquire the file
                _logger.LogInfo($"CACHE: Bundle is invalid, (re-)acquiring {bundle.FileName}");
                return true;
            }
        }
        else
        {
            // file doesn't exist in cache
            _logger.LogInfo($"CACHE: Bundle is missing, (re-)acquiring {bundle.FileName}");
            return true;
        }
    }
    */

    public static async Task<bool> ShouldAcquire(BundleItem bundle)
    {
        // If this is a local bundle, never re-acquire
        if (RequestHandler.IsLocal)
        {
            _logger.LogInfo($"MOD: Loading locally {bundle.FileName}");
            return false;
        }

        var filepath = GetBundleFilePath(bundle);

        // File missing → must acquire
        if (!VFS.Exists(filepath))
        {
            _logger.LogInfo($"CACHE: Bundle is missing, (re-)acquiring {bundle.FileName}");
            return true;
        }

        // File exists
        var fileInfo = new FileInfo(filepath);
        var size = fileInfo.Length;

        // Cache hit?
        if (BundleCrcCache.TryGet(filepath, out var cached))
        {
            // Size + CRC match → trust cache
            if (cached.Size == size && cached.Crc == bundle.Crc)
            {
                _logger.LogInfo($"CACHE: Loading locally {bundle.FileName} (cached)");
                return false;
            }
        }

        // Cache miss or mismatch → compute CRC
        var data = await VFS.ReadFileAsync(filepath);
        var crc = Crc32.HashToUInt32(data);

        // Update cache
        BundleCrcCache.Update(filepath, size, crc);

        if (crc == bundle.Crc)
        {
            _logger.LogInfo($"CACHE: Loading locally {bundle.FileName}");
            return false;
        }

        _logger.LogInfo($"CACHE: Bundle is invalid, (re-)acquiring {bundle.FileName}");
        return true;
    }
}
