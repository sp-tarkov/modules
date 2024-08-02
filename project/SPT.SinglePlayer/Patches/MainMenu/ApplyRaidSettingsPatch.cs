using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class ApplyRaidSettingsPatch : ModulePatch
    {
        /// <summary>
        /// Since Scav is never meant to have the RaidSettingsWindow If you choose scav first and modify options then go to PMC you will remain Scav until you restart the game (Same other way around).
        /// This Patch Basically overwrites the original settings so they dont get set back original settings from scav page
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RaidSettingsWindow), nameof(RaidSettingsWindow.RestoreOriginalOfflineSettings));
        }

        [PatchPrefix]
        public static void PatchPrefix(ref RaidSettings ___raidSettings_0, ref RaidSettings ___raidSettings_1)
        {
            // If Raidsettingswindow is never opened it will soft lock game
            if (___raidSettings_0 == null)
            {
                return;
            }
            ___raidSettings_1.Apply(___raidSettings_0);
        }
    }
}
