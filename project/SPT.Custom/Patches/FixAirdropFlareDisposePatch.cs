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
			return AccessTools.Method(typeof(GClass2501), nameof(GClass2501.Dispose));
		}

		[PatchPrefix]
		public static void Prefix(GClass2501 __instance)
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