using SPT.Reflection.Patching;
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

namespace SPT.Custom.Patches
{
	public class CultistAmuletRemovalPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(CultistEventsClass), nameof(CultistEventsClass.method_4));
		}

		[PatchPostfix]
		private static void PatchPostfix(DamageInfo damageInfo, Player victim)
		{
			var player = damageInfo.Player.iPlayer;
			var amulet = player.FindCultistAmulet();
			if (victim.Profile.Info.Settings.Role.IsSectant() && amulet != null)
			{
				var list = (player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem as SearchableItemClass).Slots;
				var amuletslot = list.Single(x => x.ContainedItem == amulet);
				amuletslot.RemoveItem();
			}
		}

	}
}
