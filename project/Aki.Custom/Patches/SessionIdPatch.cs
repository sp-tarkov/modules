using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.UI;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.Patches
{
    public class SessionIdPatch : ModulePatch
	{
		private static PreloaderUI _preloader;

		static SessionIdPatch()
		{
			_preloader = null;
        }

        protected override MethodBase GetTargetMethod()
        {
			return PatchConstants.LocalGameType.BaseType.GetMethod("method_5", PatchConstants.PrivateFlags);
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
