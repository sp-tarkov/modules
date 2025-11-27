using System.Collections.Generic;
using System.Reflection;
using EFT.Airdrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Custom.Patches;

/// <summary>
/// This patch prevents the weird pink smoke / flares that are still in the sky the next raid if a player has just extracted
/// while the airplane is dropping a crate
/// </summary>
public class FixAirdropFlareDisposePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProjectilesCollector), nameof(ProjectilesCollector.Dispose));
    }

    [PatchPrefix]
    public static void Prefix(ProjectilesCollector __instance)
    {
        if (__instance.ActiveProjectiles == null)
        {
            return;
        }

        foreach (KeyValuePair<GameObject, float> keyValuePair in __instance.ActiveProjectiles)
        {
            Object.Destroy(keyValuePair.Key);
        }
    }
}
