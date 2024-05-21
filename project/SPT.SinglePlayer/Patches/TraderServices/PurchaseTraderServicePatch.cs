using SPT.Reflection.Patching;
using SPT.SinglePlayer.Utils.TraderServices;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPT.SinglePlayer.Patches.TraderServices
{
    public class PurchaseTraderServicePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryControllerClass), nameof(InventoryControllerClass.TryPurchaseTraderService));
        }

        [PatchPostfix]
        public static async void PatchPostFix(Task<bool> __result, ETraderServiceType serviceType, AbstractQuestControllerClass questController, string subServiceId)
        {
            bool purchased = await __result;
            if (purchased)
            {
                Logger.LogInfo($"Player purchased service {serviceType}");
                TraderServicesManager.Instance.AfterPurchaseTraderService(serviceType, questController, subServiceId);
            }
            else
            {
                Logger.LogInfo($"Player failed to purchase service {serviceType}");
            }
        }
    }
}
