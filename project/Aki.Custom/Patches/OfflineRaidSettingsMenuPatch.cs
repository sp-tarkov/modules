using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;

namespace Aki.Custom.Patches
{
    public class OfflineRaidSettingsMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RaidSettingsWindow), nameof(RaidSettingsWindow.Show));
        }

        [PatchPostfix]
        private static void PatchPostfix(UiElementBlocker ____coopModeBlocker)
        {
            // Always disable the Coop Mode checkbox
            ____coopModeBlocker.SetBlock(true, "SPT will never support Co-op");
        }
    }
}