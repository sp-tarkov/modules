using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.Performance
{
    /// <summary>
    /// Transpiler used to stop the allocation of a new <see cref="Stopwatch"/> every frame for all active AI.
    /// <para>
    /// To update the transpiler, check for the following:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>New allocation of <see cref="Stopwatch"/></description>
    ///   </item>
    ///   <item>
    ///     <description>Calls to <see cref="Stopwatch.Start"/> and <see cref="Stopwatch.Stop"/></description>
    ///   </item>
    ///   <item>
    ///     <description>Unnecessary execution of <see cref="BotUnityEditorRunChecker.ManualLateUpdate"/></description>
    ///   </item>
    /// </list>
    /// </summary>
    public class RemoveStopwatchAllocationsEveryBotFramePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.UpdateManual));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Type stopWatchType = typeof(Stopwatch);

            CodeMatcher matcher = new(instructions, generator);
            matcher.Start();

            CodeMatch[] inst1 = [
                new(OpCodes.Newobj, stopWatchType.GetConstructor([])),
                new(OpCodes.Dup),
                new(OpCodes.Callvirt, stopWatchType.GetMethod(nameof(Stopwatch.Start)))
            ];

            matcher.MatchForward(false, inst1)
                .ThrowIfInvalid("Could not find Stopwatch allocation in IL instructions");

            matcher.InstructionAt(inst1.Length).labels.AddRange(matcher.Instruction.labels);
            matcher.Instruction.labels.Clear();
            matcher.RemoveInstructions(inst1.Length);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, stopWatchType.GetMethod(nameof(Stopwatch.Stop))))
                .ThrowIfInvalid("Could not find Stopwatch.Stop method in IL instructions");

            matcher.RemoveInstruction();

            CodeMatch[] inst2 = [
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(BotOwner).GetProperty(nameof(BotOwner.UnityEditorRunChecker)).GetGetMethod()),
                new(OpCodes.Callvirt, typeof(BotUnityEditorRunChecker).GetMethod(nameof(BotUnityEditorRunChecker.ManualLateUpdate)))
            ];

            matcher.MatchForward(false, inst2)
                .ThrowIfInvalid("Could not find call to BotUnityEditorRunChecker.ManualLateUpdate in IL instructions");

            matcher.InstructionAt(inst2.Length).labels.AddRange(matcher.Instruction.labels);
            matcher.Instruction.labels.Clear();
            matcher.RemoveInstructions(inst2.Length);

            return matcher.Instructions();
        }
    }
}
