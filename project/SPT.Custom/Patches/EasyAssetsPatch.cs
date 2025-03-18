using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Diz.Resources;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using DependencyGraph = DependencyGraphClass<IEasyBundle>;
using SPT.Reflection.Utils;

namespace SPT.Custom.Patches
{
    public class EasyAssetsPatch : ModulePatch
    {
        private static readonly FieldInfo _bundlesField;

        static EasyAssetsPatch()
        {
            _bundlesField = typeof(EasyAssets).GetFields(PatchConstants.PrivateFlags).FirstOrDefault(field => field.FieldType == typeof(EasyAssetHelperClass[]));
        }

        public EasyAssetsPatch()
        {
            _ = nameof(IEasyBundle.SameNameAsset);
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BundleLockClass.MaxConcurrentOperations);
            _ = nameof(DependencyGraph.GetDefaultNode);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EasyAssets).GetMethod(nameof(EasyAssets.Create));
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Task<EasyAssets> __result, GameObject gameObject, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
            string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        {
            var easyAsset = gameObject.AddComponent<EasyAssets>();
            __result = Init(easyAsset, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);

            return false; // Skip original
        }

        private static async Task<EasyAssets> Init(EasyAssets instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath, string platformName, [CanBeNull] Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            // platform manifest
            var eftBundlesPath = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";
            var filepath = eftBundlesPath + platformName;
            var jsonfile = filepath + ".json";
            var manifest = VFS.Exists(jsonfile)
                ? await GetManifestJson(jsonfile)
                : await GetManifestBundle(filepath);

            // lazy-initialize SPT bundles
            if (BundleManager.Bundles.Keys.Count == 0)
            {
                await BundleManager.DownloadManifest();
            }

            // create bundles array from obfuscated type
            var bundleNames = manifest.GetAllAssetBundles()
                .Union(BundleManager.Bundles.Keys)
                .ToArray();

            // create bundle lock
            if (bundleLock == null)
            {
                bundleLock = new BundleLockClass(int.MaxValue);
            }

            var bundles = new EasyAssetHelperClass[bundleNames.Length];

            var bundleUtils = BundleUtils.Create();
            bundleUtils.Init(bundleNames.Length);

            for (var i = 0; i < bundleNames.Length; i++)
            {
                var key = bundleNames[i];
                var path = eftBundlesPath;

                // acquire external bundle
                if (BundleManager.Bundles.TryGetValue(key, out var bundleInfo))
                {
                    bundleUtils.SetProgress(i, bundleInfo.FileName);

                    // we need base path without file extension
                    path = BundleManager.GetBundlePath(bundleInfo);

                    // only download when connected externally
                    if (await BundleManager.ShouldAcquire(bundleInfo))
                    {
                        if (VFS.Exists(BundleManager.GetBundleFilePath(bundleInfo)))
                        {
                            VFS.DeleteFile(BundleManager.GetBundleFilePath(bundleInfo));
                        }

                        await BundleManager.DownloadBundle(bundleInfo);
                    }
                }

                // create bundle of obfuscated type
                bundles[i] = new EasyAssetHelperClass(
                    key,
                    path,
                    manifest,
                    bundleLock,
                    bundleCheck);
            }

            bundleUtils.Dispose();

            // create dependency graph
            instance.Manifest = manifest;
            _bundlesField.SetValue(instance, bundles);
            instance.System = new DependencyGraph(bundles, defaultKey, shouldExclude);

            return instance;
        }

        // NOTE: used by:
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/cubemaps
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/defaultmaterial
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/dissonancesetup
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/Doge
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/shaders
        private static async Task<CompatibilityAssetBundleManifest> GetManifestBundle(string filepath)
        {
            var manifestLoading = AssetBundle.LoadFromFileAsync(filepath);
            await manifestLoading.Await();

            var assetBundle = manifestLoading.assetBundle;
            var assetLoading = assetBundle.LoadAllAssetsAsync();
            await assetLoading.Await();

            return (CompatibilityAssetBundleManifest)assetLoading.allAssets[0];
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestJson(string filepath)
        {
            var text = await VFS.ReadTextFileAsync(filepath);

            /* we cannot parse directly as <string, BundleDetails>, because...
                    [Error  : Unity Log] JsonSerializationException: Expected string when reading UnityEngine.Hash128 type, got 'StartObject' <>. Path '['assets/content/weapons/animations/simple_animations.bundle'].Hash', line 1, position 176.
               ...so we need to first convert it to a slimmed-down type (BundleItem), then convert back to BundleDetails.
            */
            var raw = JsonConvert.DeserializeObject<Dictionary<string, BundleItem>>(text);
            var converted = raw.ToDictionary(GetPairKey, GetPairValue);

            // initialize manifest
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(converted);

            return manifest;
        }

        public static string GetPairKey(KeyValuePair<string, BundleItem> x)
        {
            return x.Key;
        }

        public static BundleDetails GetPairValue(KeyValuePair<string, BundleItem> x)
        {
            return new BundleDetails
            {
                FileName = x.Value.FileName,
                Crc = x.Value.Crc,
                Dependencies = x.Value.Dependencies
            };
        }
    }
}
