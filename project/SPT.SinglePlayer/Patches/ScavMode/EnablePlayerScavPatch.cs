using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.ScavMode;

/// <summary>
/// This patch removes the if check if the player is a scav, it will allow for playing scavs in offline raids.
/// </summary>
public class EnablePlayerScavPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MainMenuControllerClass.Struct444), nameof(MainMenuControllerClass.Struct444.MoveNext));
    }


    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        var instructionsList = new List<CodeInstruction>(originalInstructions);

        // Remove the condition in the if check checking if the player is a scav
        instructionsList.RemoveRange(143, 4);

        return instructionsList;
    }
}
