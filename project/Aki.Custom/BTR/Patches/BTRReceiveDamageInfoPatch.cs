using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;

namespace Aki.Custom.BTR.Patches
{
    /// <summary>
    /// Patches an empty method in <see cref="BTRView"/> to handle updating the BTR bot's Neutrals and Enemies lists in response to taking damage.
    /// </summary>
    public class BTRReceiveDamageInfoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRView), nameof(BTRView.method_1));
        }

        [PatchPrefix]
        private static void PatchPrefix(DamageInfo damageInfo)
        {
            var botEventHandler = Singleton<BotEventHandler>.Instance;
            if (botEventHandler == null)
            {
                Logger.LogError($"[AKI-BTR] BTRReceiveDamageInfoPatch - BotEventHandler is null");
                return;
            }

            var shotBy = (Player)damageInfo.Player.iPlayer;
            botEventHandler.InterruptTraderServiceBtrSupportByBetrayer(shotBy);
        }
    }
}
