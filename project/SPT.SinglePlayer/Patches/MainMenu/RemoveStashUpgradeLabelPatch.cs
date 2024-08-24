using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// Remove Tooltip and image from stash screen explaining to visit external site for more stash - EFT thing
/// </summary>
public class RemoveStashUpgradeLabelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // InventoryScreen has 2 Show methods
        return AccessTools.FirstMethod(typeof(InventoryScreen), info => info.Name == "Show");
    }

    [PatchPostfix]
    public static void Postfix(SimpleStashPanel __instance)
    {
        Object.Destroy(__instance.transform.Find("Items Panel/Stash Panel/Simple Panel/Header/ExternalObtain").gameObject);
    }
}