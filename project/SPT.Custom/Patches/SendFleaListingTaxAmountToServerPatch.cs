using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Send the tax amount for listing an item for sale on flea by player to server for later use when charging player
    /// Client doesnt send this data and calculating it server-side isn't accurate
    /// </summary>
    public class SendFleaListingTaxAmountToServerPatch : ModulePatch
    {
        public SendFleaListingTaxAmountToServerPatch()
        {
            // Remember to update prefix parameter if below lines are broken
            _ = nameof(RagfairOfferSellHelperClass.IsAllSelectedItemSame);
            _ = nameof(RagfairOfferSellHelperClass.AutoSelectSimilar);
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AddOfferWindow), nameof(AddOfferWindow.method_5));
        }

        /// <summary>
        /// Calculate tax to charge player and send to server before the offer is sent
        /// </summary>
        /// <param name="___item_0">Item sold</param>
        /// <param name="___ragfairOfferSellHelperClass">OfferItemCount</param>
        /// <param name="___double_0">RequirementsPrice</param>
        /// <param name="___bool_0">SellInOnePiece</param>
        [PatchPrefix]
        public static void PatchPrefix(
            ref Item ___item_0,
            ref RagfairOfferSellHelperClass ___ragfairOfferSellHelperClass,
            ref double ___double_0,
            ref bool ___bool_0
        )
        {
            RequestHandler.PutJson(
                "/client/ragfair/offerfees",
                new
                {
                    id = ___item_0.Id,
                    tpl = ___item_0.TemplateId,
                    count = ___ragfairOfferSellHelperClass.OfferItemCount,
                    fee = Mathf.CeilToInt(
                        (float)
                            FleaTaxCalculatorAbstractClass.CalculateTaxPrice(
                                ___item_0,
                                ___ragfairOfferSellHelperClass.OfferItemCount,
                                ___double_0,
                                ___bool_0
                            )
                    ),
                }.ToJson()
            );
        }
    }
}
