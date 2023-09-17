using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.UI;
using EFT.UI.Matchmaker;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Removes the 'ready' button from the map preview screen - accessible by choosing map to deply to > clicking 'map' bottom left of screen
    /// Clicking the ready button makes a call to client/match/available, something we dont want
    /// </summary>
    public class MapReadyButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchmakerMapPointsScreen).GetMethod("Show", PatchConstants.PrivateFlags);
        }

        [PatchPostfix]
        private static void PatchPostFix(ref DefaultUIButton ____readyButton)
        {
            ____readyButton?.GameObject?.SetActive(false);
        }
    }
}