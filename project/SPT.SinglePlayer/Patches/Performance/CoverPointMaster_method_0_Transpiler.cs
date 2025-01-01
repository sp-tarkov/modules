using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.SinglePlayer.Patches.Performance
{
	/// <summary>
	/// Transpiler used to stop the allocation of a new <see cref="Stopwatch"/> during <see cref="CoverPointMaster.method_0(CoverSearchData)"/> <br/>
	/// To update transpiler, look for: <br/>
	/// - New allocation of <see cref="Stopwatch"/> <br/>
	/// - <see cref="Stopwatch.Start"/> and <see cref="Stopwatch.Stop"/> <br/>
	/// </summary>
	internal class CoverPointMaster_method_0_Transpiler : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(CoverPointMaster), nameof(CoverPointMaster.method_0));
		}

		[PatchTranspiler]
		public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> codeList = instructions.ToList();
			// This is the line that stops the Stopwatch
			codeList[69].opcode = OpCodes.Nop;

			// These lines stops the allocation and Start() of the Stopwatch
			codeList[12].opcode = OpCodes.Nop;
			codeList[11].opcode = OpCodes.Nop;
			codeList[10].opcode = OpCodes.Nop;

			return codeList;
		}
	}
}
