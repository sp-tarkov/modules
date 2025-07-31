using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// All we want to do with this TranspilePatch is to replace the Ref ID they check for to something different
/// so this allows the Ref trader to be shown on the TraderScreensGroup
/// </summary>
public class EnableRefForPVEPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TraderScreensGroup), nameof(TraderScreensGroup.method_4));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> PatchTranspile(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var searchCode = new CodeInstruction(OpCodes.Ldstr, "6617beeaa9cfa777ca915b7c");
        var searchIndex = -1;

        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == searchCode.opcode && codes[i]?.operand == searchCode?.operand)
            {
                searchIndex = i;
                break;
            }
        }

        if (searchIndex == -1)
        {
            Logger.LogError($"{nameof(EnableRefForPVEPatch)} failed: Could not find reference code.");
            return instructions;
        }

        // this doesnt have to be anything particular for the string - just cant be a trader ID
        var newCode = new CodeInstruction(OpCodes.Ldstr, "SPT-PVE");

        codes.RemoveAt(searchIndex);
        codes.Insert(searchIndex, newCode);

        return codes.AsEnumerable();
    }
}
