using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// The purpose of this patch is to remove bosses from the spawning pool if they are turned off, but not PMCs (as they are seen as bosses)
/// </summary>
public class FixDisableBossSpawningOptionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.smethod_8));
    }

    [PatchPrefix]
    public static bool PatchPrefix(WavesSettings wavesSettings, BossLocationSpawn[] bossLocationSpawn, ref BossLocationSpawn[] __result)
    {
        // We only need to filter out the bosses here, not the PMCs
        if (!wavesSettings.IsBosses)
        {
            __result = Array.FindAll(bossLocationSpawn, boss => boss.BossName is "pmcUSEC" or "pmcBEAR");
        }

        // Skip the original here, the original doesn't run any code anyway due to checks against if we are in PVE and offline
        return false;
    }
}
