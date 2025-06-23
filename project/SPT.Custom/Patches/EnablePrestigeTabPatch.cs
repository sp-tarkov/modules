using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// This patch sets the Prestige Tab to be enabled in PvE mode
    /// </summary>
    public class EnablePrestigeTabPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryScreen.Class2792), nameof(InventoryScreen.Class2792.MoveNext));
        }

        [PatchPostfix]
        public static void Postfix(InventoryScreen.Class2792 __instance)
        {
            var inventoryScreen = __instance.inventoryScreen_0;
            var tabDictionary = Traverse.Create(inventoryScreen).Field<IReadOnlyDictionary<EInventoryTab, Tab>>("_tabDictionary").Value;
            var prestigeTab = tabDictionary[EInventoryTab.Prestige];
            prestigeTab.gameObject.SetActive(true);
        }
    }
}