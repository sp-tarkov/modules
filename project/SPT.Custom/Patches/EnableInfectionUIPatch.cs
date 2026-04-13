using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.Custom.Patches;

public class EnableInfectionUIPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationConditionsPanel), nameof(LocationConditionsPanel.Set));
    }

    [PatchPrefix]
    public static void PatchPrefix(
        RaidSettings raidSettings,
        LocationConditionsPanel __instance,
        ref ZombieEventLocationInfectionInfo ____infectionInfo
    )
    {
        if (!Singleton<GlobalConfiguration>.Instance.SeasonActivityConfig.InfectionHalloweenConfig.DisplayUIEnabled)
        {
            return;
        }

        if (raidSettings.SelectedLocation is null || ____infectionInfo is null)
        {
            return;
        }

        __instance.method_0(raidSettings.SelectedLocation);
    }
}
