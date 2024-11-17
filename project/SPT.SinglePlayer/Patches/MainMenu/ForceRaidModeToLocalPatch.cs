using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.MainMenu
{
	/// <summary>
	/// This patch ensures that the gamemode is always <see cref="ERaidMode.Local"/> and that IsPveOffline is always true when starting a game<br/>
	/// This prevents a bug where the gameworld is instantiated as an online world
	/// </summary>
	public class ForceRaidModeToLocalPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.method_41));
		}

		[PatchPrefix]
		public static void Prefix(ref RaidSettings ____raidSettings)
		{
			____raidSettings.RaidMode = ERaidMode.Local;
			____raidSettings.IsPveOffline = true;
		}
	}
}
