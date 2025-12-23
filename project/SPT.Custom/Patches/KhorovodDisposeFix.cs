using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

public class KhorovodDisposeFix : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RunddansControllerAbstractClass), nameof(RunddansControllerAbstractClass.Dispose));
    }

    [PatchTranspiler]
    protected static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        var instructionsList = new List<CodeInstruction>(originalInstructions);

        instructionsList.RemoveRange(23, 3);

        return instructionsList;
    }
}
