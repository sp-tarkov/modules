using Aki.Common.Http;
using Aki.Reflection.Patching;
using System.Reflection;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using EFT.UI.Ragfair;
using UnityEngine;

namespace Aki.Custom.Patches
{
    public class RagfairFeePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(AddOfferWindow).GetMethod("method_1", PatchConstants.PrivateFlags);

        [PatchPrefix]
        private static void PatchPrefix(ref Item ___item_0, ref GClass2842 ___gclass2842_0, ref double ___double_0, ref bool ___bool_0)
        {
            RequestHandler.PutJson("/client/ragfair/offerfees", new
                {
                    id = ___item_0.Id,
                    tpl = ___item_0.TemplateId,
                    count = ___gclass2842_0.OfferItemCount,
                    fee = Mathf.CeilToInt((float) GClass1932.CalculateTaxPrice(___item_0, ___gclass2842_0.OfferItemCount, ___double_0, ___bool_0))
                }
                .ToJson());
        }
    }
}