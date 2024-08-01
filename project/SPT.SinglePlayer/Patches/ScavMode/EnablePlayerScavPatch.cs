using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class EnablePlayerScavPatch : ModulePatch
    {

        private static RaidSettings StoredRaidsettings { get; set; }
        private static RaidSettings StoredRaidsettings1 { get; set; }
		/// <summary>
		/// Modifys the raidsettings to retain raidsettings options in menu and allows scav to load into raid
		/// </summary>
		protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_22));
        }

		[PatchPrefix]
		private static void PatchPrefix(ref RaidSettings ___raidSettings_0, ref RaidSettings ___raidSettings_1, MainMenuController __instance)
		{
			StoredRaidsettings = ___raidSettings_0;
			StoredRaidsettings1 = ___raidSettings_1;
			___raidSettings_0.RaidMode = ERaidMode.Local;
			___raidSettings_0.IsPveOffline = true;
			___raidSettings_0.WavesSettings = ___raidSettings_1.WavesSettings;
			___raidSettings_0.BotSettings = ___raidSettings_1.BotSettings;
			___raidSettings_1.Apply(___raidSettings_0);
		}
		[PatchPostfix]
		private static void PatchPostfix(ref RaidSettings ___raidSettings_0)
		{
			___raidSettings_0.RaidMode = ERaidMode.Online;
			___raidSettings_0.IsPveOffline = true;
		}
	}
}
