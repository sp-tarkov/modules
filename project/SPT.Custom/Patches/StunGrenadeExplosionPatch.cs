using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace SPT.Custom.Patches
{
	/// <summary>
	/// BSG has had the wonderful idea of not letting grenades explode.
	/// Waiting on BSG Fix.
	/// </summary>
	public class StunGrenadeExplosionPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(ObservedStunGrenade), nameof(ObservedStunGrenade.StartTimer));
		}

		[PatchPrefix]
		private static bool PatchPrefix(ObservedStunGrenade __instance, float ___float_4)
		{
			__instance.StartBehaviourTimer(__instance.WeaponSource.GetExplDelay - ___float_4, new Action(__instance.InvokeBlowUpEvent));
			return false;
		}
	}

	public class GrenadeExplosionPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(ObservedGrenade), nameof(ObservedGrenade.StartTimer));
		}

		[PatchPrefix]
		private static bool PatchPrefix(ObservedGrenade __instance, float ___float_4)
		{
			__instance.StartBehaviourTimer(__instance.WeaponSource.GetExplDelay - ___float_4, new Action(__instance.InvokeBlowUpEvent));
			return false;
		}
	}
}
