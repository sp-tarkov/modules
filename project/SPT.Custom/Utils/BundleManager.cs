using System.Collections.Concurrent;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;

namespace SPT.Custom.Utils;

public static class BundleManager
{
    private const string CachePath = "user/cache/bundles/";
    private static readonly ManualLogSource _logger;
    public static readonly ConcurrentDictionary<string, BundleItem> Bundles;

    static BundleManager()
    {
        _logger = Logger.CreateLogSource(nameof(BundleManager));
        Bundles = new ConcurrentDictionary<string, BundleItem>();
    }

    public static string GetBundlePath(BundleItem bundle)
    {
        return RequestHandler.IsLocal ? $"{bundle.ModPath}/bundles/" : CachePath;
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

    public static async Task DownloadBundle(BundleItem bundle)
    {
        var filepath = GetBundleFilePath(bundle);
        var data = await RequestHandler.GetDataAsync($"/files/bundle/{bundle.FileName}");
        await VFS.WriteFileAsync(filepath, data);
    }

    // Handles both the check for initially acquiring and also re-acquiring a file.
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
}
