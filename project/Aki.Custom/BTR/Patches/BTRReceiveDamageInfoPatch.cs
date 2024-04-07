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
            return AccessTools.FirstMethod(typeof(BTRView), IsTargetMethod);
        }

        private bool IsTargetMethod(MethodBase method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 1
                && parameters[0].ParameterType == typeof(DamageInfo)
                && parameters[0].Name == "damageInfo";
        }

        [PatchPrefix]
        private static void PatchPrefix(DamageInfo damageInfo)
        {
            var botEventHandler = Singleton<BotEventHandler>.Instance;
            if (botEventHandler == null)
            {
                Logger.LogError($"[AKI-BTR] {nameof(BTRReceiveDamageInfoPatch)} - BotEventHandler is null");
                return;
            }

            var shotBy = (Player)damageInfo.Player.iPlayer;
            if (shotBy != null)
            {
                botEventHandler.InterruptTraderServiceBtrSupportByBetrayer(shotBy);
            }
        }
    }
}
