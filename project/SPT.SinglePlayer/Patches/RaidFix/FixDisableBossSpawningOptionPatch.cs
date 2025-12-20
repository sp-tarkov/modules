using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// The purpose of this patch is to remove the isPVEOffline check from the if statement 
/// </summary>
public class FixDisableBossSpawningOptionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.smethod_8));
    }


    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        var instructionsList = new List<CodeInstruction>(originalInstructions);

        instructionsList.RemoveRange(3, 2);

        return instructionsList;
    }
}
