using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// This patch removes the quest controller as well as the achievement controller being initialized in the hideout
/// In 4.0 this code was re-added for whatever reason in the hideout, this breaks certain quests and achievements needing to be completed in one life
/// </summary>
public class DisableHideoutCounterResetPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
           typeof(HideoutGame),
           nameof(HideoutGame.smethod_6)
       );
    }

    [PatchTranspiler]
    protected static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        List<CodeInstruction> modifiedInstructions = new(originalInstructions);

        for (var i = 0; i < modifiedInstructions.Count - 1; i++)
        {
            // Look for Stfld
            if (modifiedInstructions[i].opcode == OpCodes.Stfld)
            {
                // And then look for Ldarg_2 after, we can start to Nop this here as we dont need any instructions after Stfld before Ret
                if (modifiedInstructions[i + 1].opcode == OpCodes.Ldarg_2)
                {
                    for (var j = i + 1; j < modifiedInstructions.Count; j++)
                    {
                        if (modifiedInstructions[j].opcode == OpCodes.Ret)
                        {
                            break;
                        }

                        modifiedInstructions[j].opcode = OpCodes.Nop;
                        modifiedInstructions[j].operand = null;
                    }

                    break;
                }
            }
        }

        return modifiedInstructions;
    }
}
