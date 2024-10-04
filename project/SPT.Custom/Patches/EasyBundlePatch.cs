﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Diz.DependencyManager;
using UnityEngine.Build.Pipeline;
using SPT.Custom.Models;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches
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
        public static void PatchPostfix(object __instance, string key, string rootPath, CompatibilityAssetBundleManifest manifest, IBundleLock bundleLock)
        {
            var filepath = rootPath + key;
            var dependencies = manifest.GetDirectDependencies(key) ?? Array.Empty<string>();

            if (BundleManager.Bundles.TryGetValue(key, out BundleItem bundle))
            {
                // server bundle
                dependencies = (dependencies.Length > 0)
                    ? dependencies.Union(bundle.Dependencies).ToArray()
                    : bundle.Dependencies;

                // set path to either cache (HTTP) or mod (local)
                filepath = BundleManager.GetBundleFilePath(bundle);
            }

            _ = new EasyBundleHelper(__instance)
            {
                Key = key,
                Path = filepath,
                KeyWithoutExtension = Path.GetFileNameWithoutExtension(key),
                DependencyKeys = dependencies,
                LoadState = new BindableState<ELoadState>(ELoadState.Unloaded),
                BundleLock = bundleLock
            };
        }
    }
}
