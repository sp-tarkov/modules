using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
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
			return typeof(GClass2408).GetMethod(nameof(GClass2408.Dispose));
		}

		[PatchPrefix]
		public static void Prefix(Dictionary<GameObject, float> ___dictionary_0)
		{
			if (___dictionary_0 == null)
			{
				return;
			}

			foreach (KeyValuePair<GameObject, float> keyValuePair in ___dictionary_0)
			{
				Object.Destroy(keyValuePair.Key);
			}
		}
	}
}
