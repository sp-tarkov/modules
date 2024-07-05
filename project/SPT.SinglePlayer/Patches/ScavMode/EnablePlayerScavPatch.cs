using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    internal class EnablePlayerScavPatch : ModulePatch
    {
        public static ERaidMode storedRaidMode;
        public static ESideType storedSide;
        public static bool storedOnlineRaidInPVE;

        /// <summary>
        /// Temporarily trick client into thinking we are PMC and in offline mode to allow loading of scavs in PVE mode
        /// </summary>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_22));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref MainMenuController __instance, ref RaidSettings ___raidSettings_0, ref ISession ___iSession)
        {
            if (!___raidSettings_0.IsScav)
            {
                return;
            }

            // Store old settings to restore them later in postfix
            storedRaidMode = ___raidSettings_0.RaidMode;
            storedSide = ___raidSettings_0.Side;
            storedOnlineRaidInPVE = ___raidSettings_0.SelectedLocation.ForceOnlineRaidInPVE;


            ___raidSettings_0.RaidMode = ERaidMode.Online;
            ___raidSettings_0.Side = ESideType.Pmc;
            ___raidSettings_0.SelectedLocation.ForceOnlineRaidInPVE = false;
        }

        [PatchPostfix]
        private static void PatchPostfix(ref MainMenuController __instance, ref RaidSettings ___raidSettings_0, ref ISession ___iSession)
        {
            if (!___raidSettings_0.IsScav)
            {
                return;
            }

            ___raidSettings_0.RaidMode = storedRaidMode;
            ___raidSettings_0.Side = storedSide;
            ___raidSettings_0.SelectedLocation.ForceOnlineRaidInPVE = storedOnlineRaidInPVE;
        }
    }
}
