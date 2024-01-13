using Aki.Reflection.Patching;
using Diz.Jobs;
using Diz.Resources;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aki.Custom.Models;
using Aki.Custom.Utils;
using DependencyGraph = DependencyGraph<IEasyBundle>;
using Aki.Reflection.Utils;

namespace Aki.Custom.Patches
{
    public class EasyAssetsPatch : ModulePatch
    {
        private static readonly FieldInfo _bundlesField;

        static EasyAssetsPatch()
        {
            _bundlesField = typeof(EasyAssets).GetField($"{EasyBundleHelper.Type.Name.ToLowerInvariant()}_0", PatchConstants.PrivateFlags);
        }

        public EasyAssetsPatch()
        {
            _ = nameof(IEasyBundle.SameNameAsset);
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BundleLock.MaxConcurrentOperations);
            _ = nameof(DependencyGraph.GetDefaultNode);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EasyAssets).GetMethods(PatchConstants.PublicDeclaredFlags).SingleCustom(IsTargetMethod);
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 6
                    && parameters[0].Name == "bundleLock"
                    && parameters[1].Name == "defaultKey"
                    && parameters[4].Name == "shouldExclude");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, EasyAssets __instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
            string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        {
            __result = Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
            return false;
        }

        private static async Task Init(EasyAssets instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
                                      string platformName, [CanBeNull] Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            // platform manifest
            var path = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";
            var filepath = path + platformName;
            var jsonPath = filepath + ".json";
            var manifest = (File.Exists(jsonPath)) ? await GetManifestJson(filepath) : await GetManifestBundle(filepath);

            // load bundles
            var bundleNames = manifest.GetAllAssetBundles().Union(BundleManager.Bundles.Keys).ToArray();
            var bundles = (IEasyBundle[])Array.CreateInstance(EasyBundleHelper.Type, bundleNames.Length);

            if (bundleLock == null)
            {
                bundleLock = new BundleLock(int.MaxValue);
            }

            for (var i = 0; i < bundleNames.Length; i++)
            {
                bundles[i] = (IEasyBundle)Activator.CreateInstance(EasyBundleHelper.Type, new object[]
                    {
                        bundleNames[i],
                        path,
                        manifest,
                        bundleLock,
                        bundleCheck
                    });

                await JobScheduler.Yield(EJobPriority.Immediate);
            }

            instance.Manifest = manifest;
            _bundlesField.SetValue(instance, bundles);
            instance.System = new DependencyGraph(bundles, defaultKey, shouldExclude);
        }

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
            var text = string.Empty;

            using (var reader = File.OpenText($"{filepath}.json"))
            {
                text = await reader.ReadToEndAsync();
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, BundleItem>>(text).ToDictionary(GetPairKey, GetPairValue);
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(data);

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
