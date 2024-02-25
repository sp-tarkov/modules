using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aki.Custom.Patches
{
	public class CultistAmuletRemovalPatch : ModulePatch
	{
		//Update GClass ref and Possibly name GClass
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(GClass456), nameof(GClass456.method_4));
		}

		[PatchPostfix]
		private static void PatchPostfix(ref DamageInfo damageInfo, Player victim)
		{
			var player = damageInfo.Player.iPlayer;
			var amulet = damageInfo.Player.iPlayer.FindCultistAmulet();
			if (victim.Profile.Info.Settings.Role.IsSectant() && amulet != null)
			{
				var list = (player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem as GClass2683).Slots;
				var amuletslot = list.Single(x => x.ContainedItem == amulet);
				amuletslot.RemoveItem();
			}
		}

	}
}
