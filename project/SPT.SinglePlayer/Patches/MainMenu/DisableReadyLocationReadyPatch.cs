using SPT.Reflection.Patching;
using EFT.UI;
using EFT.UI.Matchmaker;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Remove the ready button from select location screen,
    /// easier to remove button than fix problems caused by clicking it
    /// </summary>
    public class DisableReadyLocationReadyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MatchMakerSelectionLocationScreen), nameof(MatchMakerSelectionLocationScreen.Awake));
        }

        [PatchPostfix]
        public static void PatchPostfix(DefaultUIButton ____readyButton)
        {
            ____readyButton.Interactable = false;
        }
    }
}
