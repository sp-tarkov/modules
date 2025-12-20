using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// This patch stops tarkov from re-making it's raid settings each time if the user is transiting
/// Without this patch if a user transits while doing the Khorovod event the event status will be removed
/// And the user will be unable to complete the event
/// </summary>
public class RemoveTransitionRaidModeSetPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TarkovApplication.Struct406), nameof(TarkovApplication.Struct406.MoveNext));
    }

    [PatchTranspiler]
    protected static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        var instructionsList = new List<CodeInstruction>(originalInstructions);

        var transitionStatusField = AccessTools.Field(typeof(TarkovApplication), "transitionStatus");
        var getInTransition = AccessTools.Method(typeof(TransitionStatusStruct), "get_InTransition");

        instructionsList[30] = new CodeInstruction(OpCodes.Ldflda, transitionStatusField);
        instructionsList.Insert(31, new CodeInstruction(OpCodes.Call, getInTransition));

        return instructionsList;
    }
}
