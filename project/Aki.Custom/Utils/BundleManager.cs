using System.Collections.Concurrent;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json;
using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Custom.Models;

namespace Aki.Custom.Utils
{
    public static class BundleManager
    {
        private static ManualLogSource _logger;
        public static readonly ConcurrentDictionary<string, BundleItem> Bundles;
        public static string CachePath;

        static BundleManager()
        {
            _logger = Logger.CreateLogSource(nameof(BundleManager));
            Bundles = new ConcurrentDictionary<string, BundleItem>();
            CachePath = "user/cache/bundles/";
        }

        public static string GetBundlePath(BundleItem bundle)
        {
            return RequestHandler.IsLocal
                ? $"{bundle.ModPath}/bundles/{bundle.FileName}"
                : CachePath + bundle.FileName;
        }

        public static void GetBundles()
        {
            // get bundles
            var json = RequestHandler.GetJson("/singleplayer/bundles");
            var bundles = JsonConvert.DeserializeObject<BundleItem[]>(json);

            // register bundles
            var toDownload = new ConcurrentBag<BundleItem>();

            Parallel.ForEach(bundles, (bundle) =>
            {
                Bundles.TryAdd(bundle.FileName, bundle);

                if (ShouldReaquire(bundle))
                {
                    // mark for download
                    toDownload.Add(bundle);
                }
            });

            if (RequestHandler.IsLocal)
            {
                // loading from local mods
                _logger.LogInfo("CACHE: Loading all bundles from mods on disk.");
                return;
            }
            else
            {
                // download bundles
                // NOTE: assumes bundle keys to be unique
                Parallel.ForEach(toDownload, (bundle) =>
                {
                    // download bundle
                    var filepath = GetBundlePath(bundle);
                    var data = RequestHandler.GetData($"/files/bundle/{bundle.FileName}");
                    VFS.WriteFile(filepath, data);
                });
            }
        }

        private static bool ShouldReaquire(BundleItem bundle)
        {
            if (RequestHandler.IsLocal)
            {
                // only handle remote bundles
                return false;
            }

            // read cache
            var filepath = CachePath + bundle.FileName;

            if (VFS.Exists(filepath))
            {
                // calculate hash
                var data = VFS.ReadFile(filepath);
                var crc = Crc32.Compute(data);

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
}
