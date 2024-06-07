using SPT.Reflection.Patching;
using SPT.SinglePlayer.Models.ScavMode;
using Comfort.Common;
using EFT.InventoryLogic.BackendInventoryInteraction;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Reflection;
using System.Threading.Tasks;
using System;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    /**
     * When the user clicks "Sell All" after a scav raid, create a custom request object
     * that includes the calculated sell price
     */
    public class ScavSellAllRequestPatch : ModulePatch
    {
        private static MethodInfo _sendOperationMethod;
        private string TargetMethodName = "SellAllFromSavage";

        protected override MethodBase GetTargetMethod()
        {
            _sendOperationMethod = AccessTools.Method(typeof(ProfileEndpointFactoryAbstractClass), nameof(ProfileEndpointFactoryAbstractClass.SendOperationRightNow));

            // NEEDS FIXING
            return AccessTools.Method(typeof(ProfileEndpointFactoryAbstractClass), nameof(ProfileEndpointFactoryAbstractClass.SellAllFromSavage));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ProfileEndpointFactoryAbstractClass __instance, ref Task<IResult> __result, string playerId, string petId)
        {

            // Build request with additional information
            OwnerInfo fromOwner = new OwnerInfo
            {
                Id = petId,
                Type = EOwnerType.Profile
            };
            OwnerInfo toOwner = new OwnerInfo
            {
                Id = playerId,
                Type = EOwnerType.Profile
            };

            SellAllRequest request = new SellAllRequest
            {
                Action = "SellAllFromSavage",
                TotalValue = ScavSellAllPriceStorePatch.StoredPrice, // Retrieve value stored in earlier patch
                FromOwner = fromOwner, // Scav
                ToOwner = toOwner // PMC
            };

            // We'll re-use the same logic/methods that the base code used
            TaskCompletionSource<IResult> taskCompletionSource = new TaskCompletionSource<IResult>();
            _sendOperationMethod.Invoke(__instance, new object[] { request, new Callback(taskCompletionSource.SetResult) });
            __result = taskCompletionSource.Task;

            // Skip original
            return false;
        }
    }
}
