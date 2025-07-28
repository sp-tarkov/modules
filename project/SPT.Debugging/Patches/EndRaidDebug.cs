using System.Reflection;
using BepInEx.Logging;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace SPT.Debugging.Patches
{
    public class EndRaidDebug : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderCard), nameof(TraderCard.method_0));
        }

        [PatchPrefix]
        public static bool PatchPreFix(
            ref LocalizedText ____nickName,
            ref TMP_Text ____standing,
            ref RankPanel ____rankPanel,
            ref Profile.TraderInfo ___traderInfo_0
        )
        {
            if (____nickName.LocalizationKey == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[SPT] _nickName.LocalizationKey was null");
            }

            if (____standing.text == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[SPT] _standing.text was null");
            }

            if (____rankPanel == null)
            {
                Logger.Log(LogLevel.Error, "[SPT] _rankPanel was null, skipping method_0");
                return false; // skip original
            }

            if (___traderInfo_0?.LoyaltyLevel == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(
                    LogLevel.Error,
                    "[SPT] ___traderInfo_0 or ___traderInfo_0.LoyaltyLevel was null"
                );
            }

            if (___traderInfo_0?.MaxLoyaltyLevel == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(
                    LogLevel.Error,
                    "[SPT] ___traderInfo_0 or ___traderInfo_0.MaxLoyaltyLevel was null"
                );
            }

            return true; // Do original method
        }
    }
}
