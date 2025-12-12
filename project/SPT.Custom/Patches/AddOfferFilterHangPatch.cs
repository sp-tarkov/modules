using System.Reflection;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

/// <summary>
/// After adding an offer, the game will refresh "My Offers". Normally when viewing "My Offers", RagfairScreen will clear the filters.
/// This is important because RagFair.FilterMyOffers() does not handle the filters well.
/// Specifically, after doing a linked search and a build preset search,
/// adding an offer will attempt to compare a MongoID and an empty string,
/// which incredibly is not supported and throws an exception, putting the flea into a bad state from which it cannot recover.
///
/// There is in fact no need to refresh "My Offers" at all, unless that is the currently selected view.
/// This patch simply no-ops the entire method for any other view.
/// </summary>
public class AddOfferFilterHangPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RagFairClass), nameof(RagFairClass.FilterMyOffers));
    }

    [PatchPrefix]
    public static bool Prefix(RagFairClass __instance)
    {
        return __instance.FilterRule.ViewListType == EViewListType.MyOffers;
    }
}
