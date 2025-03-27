using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// BSG now block the use of the DevBalaclava on anything but a Dev Profile
/// this skips the entire check and allows us to use the dev balaclava on normal accounts
/// </summary>
public class DisableDevMaskCheckPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalPlayer.Struct530), nameof(LocalPlayer.Struct530.MoveNext));
    }

    [PatchTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codeInstructions = new(instructions);

        int developerCheckIndex = -1;
        int branchIndex = -1;

        // Loop through instructions to find the developer check
        for (int i = 0; i < codeInstructions.Count; i++)
        {
            // Look for the developer check (Is method call)
            if (codeInstructions[i].opcode == OpCodes.Call && codeInstructions[i].operand is MethodInfo methodInfo && methodInfo.Name == "Is")
            {
                // Check if the next opcode checks for the true condition
                if (i + 1 < codeInstructions.Count && codeInstructions[i + 1].opcode == OpCodes.Brtrue)
                {
                    developerCheckIndex = i;
                    branchIndex = i + 1;
                }
            }
        }

        if (developerCheckIndex == -1 || branchIndex == -1)
        {
            Logger.LogError($"Patch {MethodBase.GetCurrentMethod().Name} Failed: Could not find reference Code");
            return codeInstructions;
        }

        // Modify the branchIndex to an unconditional jump (br) to entirely skip the if block
        codeInstructions[branchIndex] = new CodeInstruction(OpCodes.Br, codeInstructions[branchIndex].operand);

        return codeInstructions;
    }
}	