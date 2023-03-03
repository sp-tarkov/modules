using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.UI;
using EFT.UI.Matchmaker;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Remove the ready button from select location screen
    /// </summary>
    public class SelectLocationScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MatchMakerSelectionLocationScreen);
            var desiredMethod = desiredType.GetMethod("Awake", PatchConstants.PrivateFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix(DefaultUIButton ____readyButton)
        {
            ____readyButton.Interactable = false;
        }
    }
}
