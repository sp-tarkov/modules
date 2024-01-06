using Aki.Reflection.Patching;
using EFT.UI;
using System.IO;
using System.Reflection;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace Aki.Custom.Patches
{
    public class SessionIdPatch : ModulePatch
	{
		private static PreloaderUI _preloader;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseLocalGame<GamePlayerOwner>), nameof(BaseLocalGame<GamePlayerOwner>.method_5));
        }

		[PatchPostfix]
		private static void PatchPostfix()
		{
			if (_preloader == null)
			{
				_preloader = Object.FindObjectOfType<PreloaderUI>();
			}

			if (_preloader != null)
			{
				var raidID = Path.GetRandomFileName().Replace(".", string.Empty).Substring(0, 6).ToUpperInvariant();
				_preloader.SetSessionId(raidID);
			}
		}
	}
}
