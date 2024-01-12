using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    /// <summary>
    /// Patches an empty method in <see cref="BTRView"/> to handle updating the BTR bot's Neutrals and Enemies lists in response to taking damage.
    /// </summary>
    public class BTRReceiveDamageInfoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRView), "method_1");
        }

        [PatchPrefix]
        public static void PatchPrefix(DamageInfo damageInfo)
        {
            var globalEvents = Singleton<GClass595>.Instance;
            if (globalEvents == null)
            {
                return;
            }

            var shotBy = (Player)damageInfo.Player.iPlayer;
            globalEvents.InterruptTraderServiceBtrSupportByBetrayer(shotBy);
        }
    }
}
