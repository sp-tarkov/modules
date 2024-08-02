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
		/// <summary>
		/// Modifys the raidsettings to retain raidsettings options in menu and allows scav to load into raid
		/// All these settings might not be needed but this allows pmc and scavs to load in as needed.
		/// </summary>
		protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_22));
        }

		[PatchPrefix]
		public static void PatchPrefix(ref RaidSettings ___raidSettings_0, ref RaidSettings ___raidSettings_1, MainMenuController __instance)
		{
			___raidSettings_0.RaidMode = ERaidMode.Local;
			___raidSettings_0.IsPveOffline = true;
			___raidSettings_0.WavesSettings = ___raidSettings_1.WavesSettings;
			___raidSettings_0.BotSettings = ___raidSettings_1.BotSettings;
			___raidSettings_1.Apply(___raidSettings_0);
		}

		[PatchPostfix]
		public static void PatchPostfix(ref RaidSettings ___raidSettings_0)
		{
			___raidSettings_0.RaidMode = ERaidMode.Local;
			___raidSettings_0.IsPveOffline = true;
		}
	}
}
