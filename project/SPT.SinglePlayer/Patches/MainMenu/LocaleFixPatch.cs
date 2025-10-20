using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// There are times where the locale handling code is called while there is already
/// partial locale data in place. This transpiler patch removes a check that stops
/// locale data being loaded from the server if any locale data already exists.
///
/// The patch removes the following opcodes
///     dup
///     callvirt instance string LocaleManagerClass::get_String_0()
///     callvirt instance bool LocaleManagerClass::ContainsCulture(string)
///     brtrue.s IL_01A6
/// </summary>
public class LocaleFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        var targetType = typeof(GClass2306).GetNestedTypes().FirstOrDefault(type => type.Name.Contains("Struct"));
        Logger.LogDebug($"{this.GetType().Name} Type: {targetType?.Name}");

        return AccessTools.Method(targetType, "MoveNext");
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        // Search for the code where `LocaleManagerClass::ContainsCulture` is called
        var searchCode = new CodeInstruction(
            OpCodes.Callvirt,
            AccessTools.Method(typeof(LocaleManagerClass), nameof(LocaleManagerClass.ContainsCulture))
        );

        var searchIndex = -1;
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
            {
                // Jump back 2 to get to the start of the call chain
                searchIndex = i - 2;
                break;
            }
        }

        // Failed to find the target code
        if (searchIndex == -1)
        {
            Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find reference code.");
            return instructions;
        }

        // Replace the 4 opcodes with NOP, removing the if condition and goto
        codes[searchIndex].opcode = OpCodes.Nop;
        codes[searchIndex + 1].opcode = OpCodes.Nop;
        codes[searchIndex + 2].opcode = OpCodes.Nop;
        codes[searchIndex + 3].opcode = OpCodes.Nop;

        return codes.AsEnumerable();
    }
}
