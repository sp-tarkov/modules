using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SPT.Reflection.CodeWrapper;

namespace SPT.SinglePlayer.Patches.ScavMode;

/**
 * When the user clicks "Sell All" after a scav raid, we need to calculate
 * the total "Sell All" value, and store it for retrieval in the ScavSellAllRequestPatch
 */
public class ScavSellAllPriceStorePatch : ModulePatch
{
    private static string _fenceID = "579dc571d53a0658a154fbec";
    private static string _roubleTid = "5449016a4bdc2d6f028b456f";

    private static FieldInfo _sessionField;

    public static int StoredPrice;

    protected override MethodBase GetTargetMethod()
    {
        var scavInventoryScreenType = typeof(ScavengerInventoryScreen);
        _sessionField = AccessTools.GetDeclaredFields(scavInventoryScreenType).FirstOrDefault(f => f.FieldType == typeof(ISession));

        return AccessTools.Method(typeof(ScavengerInventoryScreen), nameof(ScavengerInventoryScreen.method_4));
    }
    
    [PatchPrefix]
    public async static void PatchPrefix(ScavengerInventoryScreen __instance)
    {
        var session = _sessionField.GetValue(__instance) as ISession;
        var traderClass = session.Traders.FirstOrDefault(x => x.Id == _fenceID);

        await traderClass.RefreshAssortment(true, true);

        // gets the list of items in the inventory screen
        if (!__instance.method_3(out var items))
        {
            Logger.LogError("ScavSellAllPriceStorePatch - Could not get items from inventory screen");
        }

        var totalPrice = 0;
        foreach (var item in items)
        {
            if (item.TemplateId == _roubleTid)
            {
                totalPrice += item.StackObjectsCount;
            }
            else
            {
                totalPrice += traderClass.GetItemPriceOnScavSell(item, true);
            }
        }

        StoredPrice = totalPrice;
    }
}
