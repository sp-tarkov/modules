using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.Custom.Patches
{
	/// <summary>
	/// BSG has had the wonderful idea of not letting grenades explode. Delay for the grenades are really long for some reason.
	/// Waiting on BSG Fix.
	/// </summary>
	public class StunGrenadeExplosionPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(ObservedStunGrenade), nameof(ObservedStunGrenade.method_4));
		}

		[PatchPrefix]
		private static bool PatchPrefix(ObservedStunGrenade __instance)
		{
			__instance.InvokeBlowUpEvent();
			return false;
		}
	}

	public class GrenadeExplosionPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(ObservedGrenade), nameof(ObservedGrenade.method_4));
		}

		[PatchPrefix]
		private static bool PatchPrefix(ObservedGrenade __instance)
		{
			__instance.InvokeBlowUpEvent();
			return false;
		}
	}
}
