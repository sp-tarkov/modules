using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using EFT.UI.Matchmaker;
using UnityEngine;

namespace Aki.Custom.Patches
{
    public class OfflineRaidSettingsMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(RaidSettingsWindow);
            var desiredMethod = desiredType.GetMethod(nameof(RaidSettingsWindow.Show));

            Logger.LogDebug($"{GetType().Name} Type: {desiredType.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            // Always disable the Coop Mode checkbox
            var coopModeComponent = GameObject.Find("CoopModeCheckmarkBlocker").GetComponent<UiElementBlocker>();
            coopModeComponent.SetBlock(true, "SPT will never support Co-op");
        }
    }
}