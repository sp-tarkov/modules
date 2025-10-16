using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Diz.DependencyManager;
using UnityEngine.Build.Pipeline;
using SPT.Custom.Models;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using HarmonyLib;

namespace SPT.Custom.Patches;

public class EasyBundlePatch : ModulePatch
{
    static EasyBundlePatch()
    {
        _ = nameof(IEasyBundle.SameNameAsset);
        _ = nameof(IBundleLock.IsLocked);
        _ = nameof(BindableStateClass<ELoadState>.Bind);
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.GetDeclaredConstructors(typeof(EasyAssetHelperClass)).First();
    }

    [PatchPostfix]
    public static void PatchPostfix(EasyAssetHelperClass __instance, string key, string rootPath, CompatibilityAssetBundleManifest manifest, IBundleLock bundleLock)
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
        __instance.String_1 = filepath;
        __instance.String_0 = Path.GetFileNameWithoutExtension(key);
        __instance.DependencyKeys = dependencies;
        __instance.LoadState = new BindableStateClass<ELoadState>(ELoadState.Unloaded);
        __instance.IBundleLock = bundleLock;
    }
}
