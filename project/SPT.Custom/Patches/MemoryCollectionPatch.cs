using System;
using System.Reflection;
using System.Runtime;
using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Screens;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine.Scripting;

namespace SPT.Custom.Patches;

public class MemoryCollectionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuTaskBar), nameof(MenuTaskBar.OnScreenChanged));
    }

    [PatchPostfix]
    public static void PatchPostfix(EEftScreenType eftScreenType)
    {
        if (eftScreenType != EEftScreenType.Inventory || !Singleton<GameWorld>.Instantiated) return;

        // Logger.LogDebug($"Running memory collection;");
        // Logger.LogDebug($"Allocated Managed Memory Before Collection: {GC.GetTotalMemory(false) / 1024f / 1024f}");

        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();

        // Logger.LogDebug($"Allocated Managed Memory After Collection: {GC.GetTotalMemory(false) / 1024f / 1024f}");
    }
}