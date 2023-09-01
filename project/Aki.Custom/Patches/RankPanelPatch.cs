using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.UI;
using System.Reflection;

namespace Aki.Custom.Patches
{
    public class RankPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(RankPanel);
            var desiredMethod = desiredType.GetMethod("Show", PatchConstants.PublicFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        private static bool PatchPreFix(ref int rankLevel, ref int maxRank)
        {
            if (Singleton<GameWorld>.Instance != null)
            {
                Logger.LogWarning("Rank Level: " + rankLevel.ToString() + " Max Rank Level: " + maxRank.ToString());
                ConsoleScreen.LogError("Rank Level: " + rankLevel.ToString() + " Max Rank Level: " + maxRank.ToString());
                ConsoleScreen.LogError("Game Broke!");
                Logger.LogWarning("This Shouldn't happen!! Please report this in discord");
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                return false;
            }
            return true;
        }
    }
}