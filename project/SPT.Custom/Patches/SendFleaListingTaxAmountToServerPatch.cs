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
    /// <param name="____selectedItem">Item sold</param>
    /// <param name="____offerContext">OfferItemCount</param>
    /// <param name="____requirementsCost">RequirementsPrice</param>
    /// <param name="____sellInOnePiece">SellInOnePiece</param>
    [PatchPrefix]
    public static void PatchPrefix(
        ref Item ____selectedItem,
        ref RagfairNewOfferContext ____offerContext,
        ref double ____requirementsCost,
        ref bool ____sellInOnePiece
    )
    {
        RequestHandler.PutJson(
            "/client/ragfair/offerfees",
            new
            {
                id = ____selectedItem.Id,
                tpl = ____selectedItem.TemplateId,
                count = ____offerContext.MaxAvailableCellsSize,
                fee = Mathf.CeilToInt(
                    (float)
                        PriceCalculator.CalculateTaxPrice(
                            ____selectedItem,
                            1, // TODO: fix this and count above, just done this to get to Vtables
                            ____requirementsCost,
                            ____sellInOnePiece
                        )
                ),
            }.ToJson()
        );
    }
}
