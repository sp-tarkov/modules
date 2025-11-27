using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Custom.Patches;

/// <summary>
/// Send the tax amount for listing an item for sale on flea by player to server for later use when charging player
/// Client doesnt send this data and calculating it server-side isn't accurate
/// </summary>
public class SendFleaListingTaxAmountToServerPatch : ModulePatch
{
    public SendFleaListingTaxAmountToServerPatch()
    {
        // Remember to update prefix parameter if below lines are broken
        _ = nameof(RagfairNewOfferContext.IsAllSelectedItemSame);
        _ = nameof(RagfairNewOfferContext.AutoSelectSimilar);
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(AddOfferWindow), nameof(AddOfferWindow.method_5));
    }

    /// <summary>
    /// Calculate tax to charge player and send to server before the offer is sent
    /// </summary>
    /// <param name="___selectedItem">Item sold</param>
    /// <param name="___offerContext">OfferItemCount</param>
    /// <param name="___requirementsCost">RequirementsPrice</param>
    /// <param name="___sellInOnePiece">SellInOnePiece</param>
    [PatchPrefix]
    public static void PatchPrefix(
        ref Item ___selectedItem,
        ref RagfairNewOfferContext ___offerContext,
        ref double ___requirementsCost,
        ref bool ___sellInOnePiece
    )
    {
        RequestHandler.PutJson(
            "/client/ragfair/offerfees",
            new
            {
                id = ___selectedItem.Id,
                tpl = ___selectedItem.TemplateId,
                count = ___offerContext.OfferItemCount,
                fee = Mathf.CeilToInt(
                    (float)
                        PriceCalculator.CalculateTaxPrice(
                            ___selectedItem,
                            ___offerContext.OfferItemCount,
                            ___requirementsCost,
                            ___sellInOnePiece
                        )
                ),
            }.ToJson()
        );
    }
}
