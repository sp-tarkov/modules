using System.Reflection;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Removes the 'ready' button from various map screens
    /// Clicking the ready button has various weird effects
    /// Such as the insurance screen when playing as a scav coming up or making calls to the back-end that we dont want.
    /// </summary>
    public static class ReadyButtonPatches
    {
        public static void Patch()
        {
            new DisableReadyLocationReadyPatch().Enable();
            new DisableMatchmakerOfflineRaidReadyPatch().Enable();
            new DisableMatchmakerInsuranceReadyPatch().Enable();
        }

        /// <summary>
        /// Disables the ready button from select location screen,
        /// easier to disable the button than fix problems caused by clicking it
        /// </summary>
        public class DisableReadyLocationReadyPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(
                    typeof(MatchMakerSelectionLocationScreen),
                    nameof(MatchMakerSelectionLocationScreen.Awake)
                );
            }

            [PatchPostfix]
            public static void PatchPostfix(DefaultUIButton ____readyButton)
            {
                ____readyButton.Interactable = false;
            }
        }

        /// <summary>
        /// Disables the ready button on the "practice game mode" screen
        /// </summary>
        public class DisableMatchmakerOfflineRaidReadyPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(
                    typeof(MatchmakerOfflineRaidScreen),
                    nameof(MatchmakerOfflineRaidScreen.Awake)
                );
            }

            [PatchPostfix]
            public static void PatchPostfix(DefaultUIButton ____readyButton)
            {
                ____readyButton.Interactable = false;
            }
        }

        /// <summary>
        /// Disables the ready button on the insurance screen
        /// </summary>
        public class DisableMatchmakerInsuranceReadyPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(
                    typeof(MatchmakerInsuranceScreen),
                    nameof(MatchmakerInsuranceScreen.Awake)
                );
            }

            [PatchPostfix]
            public static void PatchPostfix(DefaultUIButton ____readyButton)
            {
                ____readyButton.Interactable = false;
            }
        }
    }
}
