using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

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
        public static void PatchPrefix(MainMenuControllerClass __instance)
        {
            if (__instance.RaidSettings_0.Side == ESideType.Pmc)
            {
                // Client does some 'online' work before realising it should be pve
                __instance.RaidSettings_0.RaidMode = ERaidMode.Online; // Sets ___raidSettings_0.Local to true
            }
            else
            {
                // Needed for scav runs
                __instance.RaidSettings_0.RaidMode = ERaidMode.Local;
            }

            // Copy values from 'good' location to raidsettings_0 to ensure the rest of raid start process uses them
            __instance.RaidSettings_0.WavesSettings = __instance.RaidSettings_1.WavesSettings;
            __instance.RaidSettings_0.BotSettings = __instance.RaidSettings_1.BotSettings;

            // Update backup to have same values as primary
            __instance.RaidSettings_1 = __instance.RaidSettings_0.Clone();
        }

        [PatchPostfix]
        public static void PatchPostfix(MainMenuControllerClass __instance)
        {
            // This ensures scav raids show as 'local' instead of 'training', works in conjunction with prefix patches' "RaidMode = local" line
            __instance.RaidSettings_0.IsPveOffline = true;

            // Bosses are never removed from pve raids, this forces the boss array to empty itself if the 'enable bosses' flag is unchecked
            if (!__instance.RaidSettings_0.WavesSettings.IsBosses)
            {
                __instance.RaidSettings_0.SelectedLocation.BossLocationSpawn = [];
            }
        }
    }
}
