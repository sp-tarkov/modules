using Aki.Reflection.Patching;
using System.Reflection;
using Aki.Reflection.Utils;
using BepInEx.Logging;
using EFT;
using EFT.UI;
using TMPro;

namespace Aki.Debugging.Patches
{
    public class EndRaidDebug : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TraderCard).GetMethod("method_0", PatchConstants.PrivateFlags);
        }

        [PatchPrefix]
        private static bool PatchPreFix(ref LocalizedText ____nickName, ref TMP_Text ____standing,
            ref RankPanel ____rankPanel, ref Profile.TraderInfo ___traderInfo_0)
        {
            if (____nickName.LocalizationKey == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] _nickName.LocalizationKey was null");
            }

            if (____standing.text == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] _standing.text was null");
            }

            if (____rankPanel == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _rankPanel was null, skipping method_0");
                return false; // skip original
            }

            if (___traderInfo_0?.LoyaltyLevel == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] ___traderInfo_0 or ___traderInfo_0.LoyaltyLevel was null");
            }

            if (___traderInfo_0?.MaxLoyaltyLevel == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] ___traderInfo_0 or ___traderInfo_0.MaxLoyaltyLevel was null");
            }

            return true;
        }
    }
}