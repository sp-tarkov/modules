using System.Collections.Generic;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

/// <summary>
/// This patch sets the Prestige Tab to be enabled in PvE mode
/// </summary>
public class EnablePrestigeTabPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // Either 5 or 4
        return AccessTools.Method(typeof(InventoryScreen.CG_Class5), nameof(InventoryScreen.CG_Class5.MoveNext));
    }

    [PatchPostfix]
    public static void Postfix(InventoryScreen.CG_Class5 __instance)
    {
        var inventoryScreen = __instance.inventoryScreen_0;
        var tabDictionary = Traverse.Create(inventoryScreen).Field<IReadOnlyDictionary<EInventoryTab, Tab>>("_tabDictionary").Value;
        var prestigeTab = tabDictionary[EInventoryTab.Prestige];
        prestigeTab.gameObject.SetActive(true);
    }
}
