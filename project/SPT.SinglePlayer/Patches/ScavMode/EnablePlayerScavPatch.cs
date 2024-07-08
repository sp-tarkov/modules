using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class EnablePlayerScavPatch : ModulePatch
    {
        /// <summary>
        /// Change Raid Mode to local and ForceOnlineRaidInPVE to true to allow loading in as a scav
        /// </summary>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_22));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref MainMenuController __instance, ref RaidSettings ___raidSettings_0, ref ISession ___iSession)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Local;
            ___raidSettings_0.SelectedLocation.ForceOnlineRaidInPVE = true;
        }
    }
}
