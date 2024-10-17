﻿using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.RaidFix
{
	/// <summary>
	/// This patch stops the player from receiving the incompatible ammo notification if it's triggered by an AI
	/// </summary>
	public class FirearmControllerShowIncompatibleNotificationClass : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Player.FirearmController.GClass1748).GetMethod(nameof(Player.FirearmController.GClass1748.ShowIncompatibleNotification));
		}

		[PatchPrefix]
		public static bool Prefix(Player ___player_0)
		{
			return ___player_0.IsYourPlayer;
		}
	}
}
