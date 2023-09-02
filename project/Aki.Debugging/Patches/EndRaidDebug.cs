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
        private static void PatchPreFix(ref LocalizedText ____nickName, ref TMP_Text ____standing,
            ref RankPanel ____rankPanel, ref Profile.GClass1618 ____gclass1618_0)
        {
            Logger.Log(LogLevel.Info, "[AKI] Logging test");

            if (____nickName.LocalizationKey == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _nickName.LocalizationKey was null");
            }

            if (____standing.text == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _standing.text was null");
            }

            if (____rankPanel == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _rankPanel was null");
            }

            if (____gclass1618_0?.LoyaltyLevel == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _gclass1618_0 or _gclass1618_0.LoyaltyLevel was null");
            }

            if (____gclass1618_0?.MaxLoyaltyLevel == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _gclass1618_0 or _gclass1618_0.MaxLoyaltyLevel was null");
            }
        }
    }
}