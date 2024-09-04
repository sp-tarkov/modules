﻿using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.MainMenu
{
	/// <summary>
	/// Remove the label shown on some of ragmans clothing options to "buy from website"
	/// </summary>
	internal class RemoveClothingItemExternalObtainLabelPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(ClothingItem).GetMethod(nameof(ClothingItem.Init));
		}

		[PatchPrefix]
		private static void Prefix(ref ClothingItem.GClass3339 offer)
		{
			offer.Offer.ExternalObtain = false;
		}
	}
}