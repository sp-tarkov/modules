using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Diz.Binding;
using Diz.DependencyManager;
using Diz.Resources;
using HarmonyLib;
using SPT.Custom.Models;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using UnityEngine.Build.Pipeline;

namespace SPT.Custom.Patches;

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
        return AccessTools.GetDeclaredConstructors(typeof(EasyBundle)).First();
    }

    [PatchPostfix]
    public static void PatchPostfix(EasyBundle __instance, string key, string rootPath, CompatibilityAssetBundleManifest manifest, IBundleLock bundleLock)
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

        __instance.Key = key;
        __instance._path = filepath;
        __instance._keyWithoutExtension = Path.GetFileNameWithoutExtension(key);
        __instance.DependencyKeys = dependencies;
        __instance.LoadState = new BindableState<ELoadState>(ELoadState.Unloaded);
        __instance._bundleLock = bundleLock;
    }
}
