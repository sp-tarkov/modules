﻿using System.Collections.Generic;
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
        private static readonly ManualLogSource _logger;
        public static readonly Dictionary<string, BundleItem> Bundles;
        public static string CachePath;

        static BundleManager()
        {
            _logger = Logger.CreateLogSource(nameof(BundleManager));
            Bundles = new Dictionary<string, BundleItem>();
            CachePath = "user/cache/bundles/";
        }

        public static string GetBundlePath(BundleItem bundle)
        {
            return RequestHandler.IsLocal
                ? $"{bundle.ModPath}/bundles/{bundle.FileName}"
                : CachePath + bundle.FileName;
        }

        public static async Task GetBundles()
        {
            // get bundles
            var json = RequestHandler.GetJson("/singleplayer/bundles");
            var bundles = JsonConvert.DeserializeObject<BundleItem[]>(json);

            // register bundles
            var toDownload = new List<BundleItem>();

            foreach (var bundle in bundles)
            {
                Bundles.Add(bundle.FileName, bundle);

                if (await ShouldReaquire(bundle))
                {
                    // mark for download
                    toDownload.Add(bundle);
                }
            }

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
                foreach (var bundle in toDownload)
                {
                    // download bundle
                    var filepath = GetBundlePath(bundle);
                    var data = await RequestHandler.GetDataAsync($"/files/bundle/{bundle.FileName}");
                    await VFS.WriteFileAsync(filepath, data);
                }
            }
        }

        private static async Task<bool> ShouldReaquire(BundleItem bundle)
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
                var data = await VFS.ReadFileAsync(filepath);
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
