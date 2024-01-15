using Aki.Reflection.Patching;
using Aki.SinglePlayer.Utils.TraderServices;
using HarmonyLib;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.TraderServices
{
    public class GetTraderServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryControllerClass), nameof(InventoryControllerClass.GetTraderServicesDataFromServer));
        }

        [PatchPrefix]
        public static bool PatchPrefix(string traderId)
        {
            Logger.LogInfo($"Loading {traderId} services from servers");
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(traderId);

            // Skip original
            return false;
        }
    }
}
