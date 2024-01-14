using System;
using Aki.Reflection.Patching;
using Diz.DependencyManager;
using UnityEngine.Build.Pipeline;
using System.IO;
using System.Linq;
using System.Reflection;
using Aki.Custom.Models;
using Aki.Custom.Utils;

namespace Aki.Custom.Patches
{
    public class EasyBundlePatch : ModulePatch
    {
        static EasyBundlePatch()
        {
            _ = nameof(IEasyBundle.SameNameAsset);
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BindableState<ELoadState>.Bind);
        }

        protected override MethodBase GetTargetMethod()
        {
            return EasyBundleHelper.Type.GetConstructors()[0];
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance, string key, string rootPath, CompatibilityAssetBundleManifest manifest, IBundleLock bundleLock)
        {
            var path = rootPath + key;
            var dependencyKeys = manifest.GetDirectDependencies(key) ?? Array.Empty<string>();

            if (BundleManager.Bundles.TryGetValue(key, out BundleInfo bundle))
            {
                dependencyKeys = (dependencyKeys.Length > 0) ? dependencyKeys.Union(bundle.DependencyKeys).ToArray() : bundle.DependencyKeys;
                path = bundle.Path;
            }

            _ = new EasyBundleHelper(__instance)
            {
                Key = key,
                Path = path,
                KeyWithoutExtension = Path.GetFileNameWithoutExtension(key),
                DependencyKeys = dependencyKeys,
                LoadState = new BindableState<ELoadState>(ELoadState.Unloaded, null),
                BundleLock = bundleLock
            };
        }
    }
}
