using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.SinglePlayer.Patches.Performance
{
    /// <summary>
    /// Transpiler used to stop the allocation of a new <see cref="Stopwatch"/> every frame for all active AI <br/>
    /// To update transpiler, look for: <br/>
    /// - New allocation of <see cref="Stopwatch"/> <br/>
    /// - <see cref="Stopwatch.Start"/> and <see cref="Stopwatch.Stop"/> <br/>
    /// - Unnecessary run of <see cref="BotUnityEditorRunChecker.ManualLateUpdate"/>
    /// </summary>
    public class RemoveStopwatchAllocationsEveryBotFramePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.UpdateManual));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeList = instructions.ToList();

            // These 3 lines remove BotUnityEditorRunChecker.ManualLateUpdate()
            codeList[115] = new CodeInstruction(OpCodes.Nop);
            codeList[114] = new CodeInstruction(OpCodes.Nop);
            codeList[113].opcode = OpCodes.Nop;

            // These 4 remove the allocation of the Stopwatch and the Start() and Stop()
            codeList[18] = new CodeInstruction(OpCodes.Nop);
            codeList[14] = new CodeInstruction(OpCodes.Nop);
            codeList[13] = new CodeInstruction(OpCodes.Nop);
            codeList[12] = new CodeInstruction(OpCodes.Nop);

            return codeList;
        }
    }
}
