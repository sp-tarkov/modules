using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using HarmonyLib;
using System.Reflection;

namespace Aki.Debugging.Patches
{
	public class ReloadClientPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(PreloaderUI), nameof(PreloaderUI.Awake));
		}

		[PatchPostfix]
		private static void PatchPostfix()
		{
			ConsoleScreen.Processor.RegisterCommandGroup<ReloadClientPatch>();
		}

		[ConsoleCommand("reload", "", null, "Reloads currently loaded profile.\nOnly use while in Main Menu" +
			"\nRunning command while in hideout will cause graphical glitches and NRE to do with Nightvision. Pretty sure wont cause anything bad" +
			"\nMay Cause Unexpected Behaviors inraid")]
		public static void Reload()
		{

			var tarkovapp = Reflection.Utils.ClientAppUtils.GetMainApp();
			GameWorld gameWorld = Singleton<GameWorld>.Instance;
			if (gameWorld != null && gameWorld.MainPlayer.Location != "hideout")
			{
				ConsoleScreen.LogError("You are in raid. Please only use in Mainmenu");
				return; // return early we dont want to cause errors because we are inraid
			}
			else if (gameWorld != null)
			{
				tarkovapp.HideoutControllerAccess.UnloadHideout();
			}
			tarkovapp.method_49();
		}
	}
}
