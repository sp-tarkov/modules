using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using TMPro;

/// <summary>
/// The LocaleManager has a race condition when attempting to merge the character map that can
/// be triggered by the Dialogue route returning while the locale route is processing. Work around the
/// race condition by branching to our own method to handle the dictionary that utilizes a copy of
/// the list of values, so it doesn't throw an exception if it's modified while we run
///
/// Note: This is in SPT.Custom because it references TMPro, which isn't available in SPT.SinglePlayer
/// </summary>

namespace SPT.Custom.Patches;
public class LocaleManagerRaceConditionFixPatch : ModulePatch
{
    private static Type fontAssetsType = typeof(LocalizationManager)
        .GetNestedTypes(PatchConstants.PublicDeclaredFlags)
        .SingleCustom(IsTargetNestedType);
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.FirstMethod(typeof(LocalizationManager), IsTargetMethod);
    }

    private bool IsTargetMethod(MethodInfo method)
    {
        // This is `method_1` in 40087
        ParameterInfo[] parameters = method.GetParameters();
        return parameters.Length == 1
            && parameters[0].ParameterType == typeof(string)
            && parameters[0].Name == "localeType";
    }

    private static bool IsTargetNestedType(Type nestedType)
    {
        return nestedType.GetFields().Length == 1
            && nestedType.GetField("mainFallBack") != null;
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        // Search for the last TryGetValue call
        var codeOffset = codes.FindLastIndex(code =>
            code.opcode == OpCodes.Callvirt && code.operand.ToString().Contains("TryGetValue")
        );

        // Failed to find the target code
        if (codeOffset == -1)
        {
            Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find reference code.");
            return instructions;
        }

        // Extract the jump location from the instruction after the if condition
        var jumpTarget = codes[codeOffset + 1].operand;

        // Extract a reference to `gclass` from the argument passed to TryGetValue
        var fontDictionaryOperand = codes[codeOffset - 1].operand;

        codeOffset += 2;
        var newCodes = CodeGenerator.GenerateInstructions(
            new List<Code>()
            {
                // LocaleManagerRaceConditionFixPatch::AddCharacters(gclass, @class.mainFallBack)
                new Code(OpCodes.Ldloc_S, fontDictionaryOperand),   // gclass
                new Code(OpCodes.Ldloc_0),                          // @class
                new Code(OpCodes.Ldfld, fontAssetsType, "mainFallBack"), // .mainFallBack
                new Code(OpCodes.Call, typeof(LocaleManagerRaceConditionFixPatch), nameof(AddCharacters)),

                // JMP past the original for loop
                new Code(OpCodes.Br_S, jumpTarget),
            }
        );
        codes.InsertRange(codeOffset, newCodes);

        return codes.AsEnumerable();
    }

    public static void AddCharacters(Dictionary<string, string> fontDictionary, TMP_FontAsset mainFallBack)
    {
        foreach (var characters in fontDictionary.Values.ToList())
        {
            mainFallBack.TryAddCharacters(characters, false);
        }
    }
}
