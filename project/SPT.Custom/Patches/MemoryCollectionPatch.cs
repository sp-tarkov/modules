using System.Reflection;
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
        if (eftScreenType != EEftScreenType.Inventory || !Singleton<GameWorld>.Instantiated)
        {
            return;
        }

        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;

        // 25000000 Nanoseconds is 25 Milliseconds.
        GarbageCollector.CollectIncremental(25000000L);
    }
}
