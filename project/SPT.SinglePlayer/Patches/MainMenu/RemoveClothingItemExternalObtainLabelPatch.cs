using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.MainMenu
{
	internal class RemoveClothingItemExternalObtainLabelPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(ClothingItem).GetMethod(nameof(ClothingItem.Init));
		}

		[PatchPrefix]
		private static void Prefix(ref ClothingItem.GClass3338 offer)
		{
			offer.Offer.ExternalObtain = false;
		}
	}
}
