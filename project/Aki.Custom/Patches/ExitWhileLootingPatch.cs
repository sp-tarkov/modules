using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// Fix a bsg bug that causes the game to soft-lock when you have a container opened when extracting
    /// </summary>
    public class ExitWhileLootingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.Single(x => x.Name == "LocalGame").BaseType // BaseLocalGame
                .GetMethod("Stop", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // Look at BaseLocalGame<TPlayerOwner> and find a method named "Stop"
        // once you find it, there should be a StartBlackScreenShow method with
        // a callback method (on dnspy will be called @class.method_0)
        // Go into that method. This will be part of the code:
        //      if (GClass2505.CheckCurrentScreen(EScreenType.Reconnect))
        //		{
        //			GClass2505.CloseAllScreensForced();
        //		}
        // The code INSIDE the if needs to run
        [PatchPrefix]
        private static bool PatchPrefix(string profileId)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            if (profileId == player?.Profile.Id)
            {
                GClass2728.Instance.CloseAllScreensForced();
            }

            return true;
        }
    }
}
