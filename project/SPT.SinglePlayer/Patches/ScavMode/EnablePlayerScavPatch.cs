using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class EnablePlayerScavPatch : ModulePatch
    {
		/// <summary>
		/// Fixes player loading into a 'practice' raid instead of a 'local' raid
		/// Also fixes player not loading into raid as a scav
		/// </summary>
		protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuControllerClass), nameof(MainMenuControllerClass.method_26));
        }

		[PatchPrefix]
		public static void PatchPrefix(ref RaidSettings ___raidSettings_0, ref RaidSettings ___raidSettings_1, MainMenuControllerClass __instance)
		{
            if (___raidSettings_0.Side == ESideType.Pmc)
            {
                // Client does some 'online' work before realising it should be pve
                ___raidSettings_0.RaidMode = ERaidMode.Online; // Sets ___raidSettings_0.Local to true
            }
            else
            {
                // Needed for scav runs
                ___raidSettings_0.RaidMode = ERaidMode.Local;
            }

            // Copy values from 'good' location to raidsettings_0 to ensure the rest of raid start process uses them
            ___raidSettings_0.WavesSettings = ___raidSettings_1.WavesSettings;
            ___raidSettings_0.BotSettings = ___raidSettings_1.BotSettings;

            // Update backup to have same values as primary
            ___raidSettings_1 = ___raidSettings_0.Clone();
        }

		[PatchPostfix]
		public static void PatchPostfix(ref RaidSettings ___raidSettings_0)
		{
            // This ensures scav raids show as 'local' instead of 'training', works in conjunction with prefix patches' "RaidMode = local" line
            ___raidSettings_0.IsPveOffline = true;

            // Bosses are never removed from pve raids, this forces the boss array to empty itself if the 'enable bosses' flag is unchecked
            if (!___raidSettings_0.WavesSettings.IsBosses)
            {
                ___raidSettings_0.SelectedLocation.BossLocationSpawn = [];
            }
        }
	}
}
