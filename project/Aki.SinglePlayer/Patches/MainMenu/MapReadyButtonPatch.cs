using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using BepInEx.Logging;
using EFT.UI;
using EFT.UI.Matchmaker;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    public class MapReadyButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchmakerMapPointsScreen).GetMethod("Show", PatchConstants.PrivateFlags);
        }

        [PatchPostfix]
        private static void PatchPostFix(ref DefaultUIButton ___readyButton)
        {
            ___readyButton?.GameObject?.SetActive(false);

            Logger.Log(LogLevel.Error, "ready button disabled");
        }
    }
}