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
        public static readonly ConcurrentDictionary<string, BundleInfo> Bundles;
        public static string CachePath;

        static BundleManager()
        {
            _logger = Logger.CreateLogSource(nameof(BundleManager));
            Bundles = new ConcurrentDictionary<string, BundleInfo>();
            CachePath = "user/cache/bundles/";
        }

        public static void GetBundles()
        {
            // detect network location
            var isLocal = RequestHandler.Host.Contains("127.0.0.1")
                       || RequestHandler.Host.Contains("localhost");

            // get bundles
            var json = RequestHandler.GetJson("/singleplayer/bundles");
            var bundles = JsonConvert.DeserializeObject<BundleItem[]>(json);

            // register bundles
            var toDownload = new ConcurrentBag<BundleItem>();

            Parallel.ForEach(bundles, (bundle) =>
            {
                // assumes loading from cache happens more often
                if (ShouldReaquire(isLocal, bundle))
                {
                    // mark for download
                    toDownload.Add(bundle);
                }
                else
                {
                    // register local bundles
                    var filepath = bundle.ModPath + bundle.FileName;
                    RegisterBundle(filepath, bundle);
                }
            });

            if (isLocal)
            {
                _logger.LogInfo("CACHE: Loading all bundles from mods on disk.");
                return;
            }

            // download bundles
            // NOTE: assumes bundle keys to be unique
            Parallel.ForEach(toDownload, (bundle) =>
            {
                var filepath = CachePath + bundle.FileName;
                var data = RequestHandler.GetData($"/files/bundle/{bundle.FileName}");

                VFS.WriteFile(filepath, data);
                RegisterBundle(filepath, bundle);
            });
        }

        private static bool ShouldReaquire(bool isLocal, BundleItem bundle)
        {
            if (isLocal)
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

                if (crc != bundle.Crc)
                {
                    // crc doesn't match, reaquire the file
                    _logger.LogInfo($"CACHE: Bundle is invalid, (re-)acquiring {bundle.FileName}");
                    return true;
                }
                else
                {
                    // file is up-to-date
                    _logger.LogInfo($"CACHE: Loading locally {bundle.FileName}");
                    return false;
                }
            }
            else
            {
                // file doesn't exist in cache
                _logger.LogInfo($"CACHE: Bundle is missing, (re-)acquiring {bundle.FileName}");
                return true;
            }            
        }

        private static void RegisterBundle(string filepath, BundleItem bundle)
        {
            var bundleInfo = new BundleInfo(bundle.FileName, filepath, bundle.Dependencies);
            Bundles.TryAdd(filepath, bundleInfo);
        }
    }
}
