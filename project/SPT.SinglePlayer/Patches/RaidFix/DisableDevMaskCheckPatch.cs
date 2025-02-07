using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using EFT;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// BSG now block the use of the DevBalaclava on anything but a Dev Profile
/// this will replace the string it checks for to a bogus string,
/// allowing us entry.
/// TODO: change to remove the whole check as its expensive for no reason. Thanks BSG
/// </summary>
public class DisableDevMaskCheckPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalPlayer.Struct522), nameof(LocalPlayer.Struct522.MoveNext));
    }

    [PatchTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        
        // Search for the code where the string "58ac60eb86f77401897560ff Name" is
        var searchCode = new CodeInstruction(OpCodes.Ldstr, "58ac60eb86f77401897560ff Name");
        var searchIndex = -1;

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
            {
                searchIndex = i;
                break;
            }
        }
        
        // Patch Failed
        if (searchIndex == -1)
        {
            Logger.LogError($"Patch {MethodBase.GetCurrentMethod().Name} Failed: Could not find reference Code");
            return instructions;
        }
        
        var newCodeToUse = new CodeInstruction(OpCodes.Ldstr, "FuCkOfFbSg");
        codes[searchIndex] = newCodeToUse;
        
        return codes.AsEnumerable();
    }
}