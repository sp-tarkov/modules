using Aki.Common.Http;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using EFT.UI.Ragfair;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// Send the tax amount for listing an item for sale on flea by player to server for later use when charging player
    /// Client doesnt send this data and calculating it server-side isn't accurate
    /// </summary>
    public class RagfairFeePatch : ModulePatch
    {
        public RagfairFeePatch()
        {
            // Remember to update prefix parameter if below lines are broken
            _ = nameof(GClass2859.IsAllSelectedItemSame);
            _ = nameof(GClass2859.AutoSelectSimilar);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(AddOfferWindow).GetMethod("method_1", PatchConstants.PrivateFlags);
        }

        /// <summary>
        /// Calculate tax to charge player and send to server before the offer is sent
        /// </summary>
        /// <param name="___item_0">Item sold</param>
        /// <param name="___gclass2859_0">OfferItemCount</param>
        /// <param name="___double_0">RequirementsPrice</param>
        /// <param name="___bool_0">SellInOnePiece</param>
        [PatchPrefix]
        private static void PatchPrefix(ref Item ___item_0, ref GClass2859 ___gclass2859_0, ref double ___double_0, ref bool ___bool_0)
        {
            RequestHandler.PutJson("/client/ragfair/offerfees", new
            {
                id = ___item_0.Id,
                tpl = ___item_0.TemplateId,
                count = ___gclass2859_0.OfferItemCount,
                fee = Mathf.CeilToInt((float)GClass1940.CalculateTaxPrice(___item_0, ___gclass2859_0.OfferItemCount, ___double_0, ___bool_0))
            }
                .ToJson());
        }
    }
}