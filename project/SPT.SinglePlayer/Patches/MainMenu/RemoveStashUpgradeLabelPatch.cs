﻿using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// Remove Tooltip and image from stash screen explaining to visit external site for more stash - EFT thing
/// </summary>
public class RemoveStashUpgradeLabelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
		return typeof(InventoryScreen).GetMethod(nameof(InventoryScreen.Awake));
    }

    [PatchPostfix]
    public static void Postfix(InventoryScreen __instance)
    {
		var externalObtain = __instance.transform.Find("Items Panel/Stash Panel/Simple Panel/TopPanel/ExternalObtain").gameObject;
		if (externalObtain != null)
		{
			Object.Destroy(externalObtain);
		}		
    }
}