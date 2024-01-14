using Aki.Debugging.BTR.Utils;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;
using System.Reflection;
using System.Threading.Tasks;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRTransferItemsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {

            return AccessTools.Method(typeof(TransferItemsInRaidScreen), nameof(TransferItemsInRaidScreen.Close));
        }

        [PatchPostfix]
        public static void PatchPostfix(bool ___bool_1)
        {
            // Didn't extract items
            if (!___bool_1)
            {
                return;
            }

            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld?.MainPlayer;
            var btrManager = gameWorld?.GetComponent<BTRManager>();
            if (gameWorld == null || player == null || btrManager == null)
            {
                Logger.LogError("[AKI-BTR] BTRTransferItemsPatch - Error fetching game objects");
                return;
            }

            // Update the trader services information now that we've used this service
            btrManager.SetServicePurchased(ETraderServiceType.BtrItemsDelivery, BTRUtil.BTRTraderId);
            BTRUtil.UpdateTraderServices(BTRUtil.BTRTraderId);
        }
    }
}
