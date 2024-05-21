using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using EFT.UI;
using EFT.UI.Matchmaker;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Removes the 'ready' button from the map preview screen - accessible by choosing map to deploy to > clicking 'map' bottom left of screen
    /// Clicking the ready button makes a call to client/match/available, something we don't want that
    /// </summary>
    public class MapReadyButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // We don't really care which "Show" method is returned - either will do
            return typeof(MatchmakerMapPointsScreen).GetMethods().First(m => m.Name == nameof(MatchmakerMapPointsScreen.Show));
        }

        [PatchPostfix]
        private static void PatchPostFix(ref DefaultUIButton ____readyButton)
        {
            ____readyButton?.GameObject?.SetActive(false);
        }
    }
}