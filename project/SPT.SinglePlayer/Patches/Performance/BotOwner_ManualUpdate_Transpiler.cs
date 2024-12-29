using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace SPT.SinglePlayer.Patches.Performance
{
	/// <summary>
	/// Transpiler used to stop the allocation of a new <see cref="Stopwatch"/> every frame for all active AI <br/>
	/// To update transpiler, look for: <br/>
	/// - New allocation of <see cref="Stopwatch"/> <br/>
	/// - <see cref="Stopwatch.Start"/> and <see cref="Stopwatch.Stop"/> <br/>
	/// - Unnecessary run of <see cref="BotUnityEditorRunChecker.ManualLateUpdate"/>
	/// </summary>
	public class BotOwner_ManualUpdate_Transpiler : ModulePatch
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
			codeList[109] = new CodeInstruction(OpCodes.Nop);
			codeList[108] = new CodeInstruction(OpCodes.Nop);
			codeList[107].opcode = OpCodes.Nop;

			// These 4 remove the allocation of the Stopwatch and the Start() and Stop()
			codeList[18] = new CodeInstruction(OpCodes.Nop);
			codeList[14] = new CodeInstruction(OpCodes.Nop);
			codeList[13] = new CodeInstruction(OpCodes.Nop);
			codeList[12] = new CodeInstruction(OpCodes.Nop);

			return codeList;
		}
	}
}
