using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class RaidSettingsScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(MatchmakerOfflineRaidScreen), IsTargetMethod);
        }

        private bool IsTargetMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 2
                && parameters[0].Name == "profileInfo"
                && parameters[1].Name == "raidSettings";
        }

        [PatchPostfix]
        private static void PatchPostfix(MatchmakerOfflineRaidScreen __instance, DefaultUIButton ____changeSettingsButton, UiElementBlocker ____onlineBlocker)
        {
            ____onlineBlocker.gameObject.SetActive(false);
            ____changeSettingsButton.Interactable = true;
            __instance.transform.Find("Content/WarningPanelHorLayout").gameObject.SetActive(false);
        }
    }
}
