using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class EnablePlayerScavPatch : ModulePatch
    {
        /// <summary>
        /// Temporarily trick client into thinking we are PMC and in offline mode to allow loading of scavs in PVE mode
        /// </summary>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_22));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Local;
            ___raidSettings_0.IsPveOffline = true;
        }
        [PatchPostfix]
        private static void PatchPostfix(ref RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Online;
            ___raidSettings_0.IsPveOffline = true;
        }
    }
}
