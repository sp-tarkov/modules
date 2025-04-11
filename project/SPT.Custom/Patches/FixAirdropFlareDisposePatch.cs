using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SPT.Custom.Patches
{
	/// <summary>
	/// This patch prevents the weird pink smoke / flares that are still in the sky the next raid if a player has just extracted
	/// while the airplane is dropping a crate
	/// </summary>
	public class FixAirdropFlareDisposePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(GClass2494), nameof(GClass2494.Dispose));
		}

		[PatchPrefix]
		public static void Prefix(GClass2494 __instance)
		{
			if (__instance.Dictionary_0 == null)
			{
				return;
			}

			foreach (KeyValuePair<GameObject, float> keyValuePair in __instance.Dictionary_0)
			{
				Object.Destroy(keyValuePair.Key);
			}
		}
	}
}
