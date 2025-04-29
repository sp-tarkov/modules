using EFT;
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
			return typeof(Player.FirearmController.GClass1840).GetMethod(nameof(Player.FirearmController.GClass1840.ShowIncompatibleNotification));
		}

		[PatchPrefix]
		public static bool Prefix(Player.FirearmController.GClass1840 __instance)
		{
			return __instance.Player_0.IsYourPlayer;
		}
	}
}