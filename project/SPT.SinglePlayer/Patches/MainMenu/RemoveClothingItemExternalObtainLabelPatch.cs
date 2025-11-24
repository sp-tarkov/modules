using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// Remove the label shown on some of Ragmans clothing options to "buy from website"
/// </summary>
public class RemoveClothingItemExternalObtainLabelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ClothingItem).GetMethod(nameof(ClothingItem.Init));
    }

    [PatchPrefix]
    private static void Prefix(ref ClothingItem.FullOffer offer)
    {
        offer.Offer.ExternalObtain = false;
    }
}
