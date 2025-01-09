using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// This transpiler removes the call to <see cref="IBackEndSession.SessionMode"/> to determine if it's <see cref="ESessionMode.Regular"/> <br/>
    /// and forces a true instead
    /// </summary>
    internal class EnablePrestigeTabPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryScreen.Class2754), nameof(InventoryScreen.Class2754.MoveNext));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] codeInstructions = instructions.ToArray();

            // Remove all instructions that gets the gamemode from iSession
            codeInstructions[259] = new(OpCodes.Nop);
            codeInstructions[260] = new(OpCodes.Nop);
            codeInstructions[261] = new(OpCodes.Nop);
            codeInstructions[262] = new(OpCodes.Nop);
            codeInstructions[263] = new(OpCodes.Nop);

            // Force a true on the stack instead
            codeInstructions[264] = new(OpCodes.Ldc_I4_1);

            return codeInstructions;
        }
    }
}
