using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
	/// <summary>
	/// This patch ensures that PMC AI is always hostile towards the player and scavs if the player it checks is a <see cref="EPlayerSide.Savage"/> (scav)
	/// </summary>
	public class ScavIsPlayerEnemyPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsPlayerEnemy));
		}

		[PatchPrefix]
		public static bool Prefix(BotsGroup __instance, IPlayer player, ref bool __result)
		{
			if (player.Side is EPlayerSide.Savage && __instance.InitialBotType is WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC)
			{
				__result = true;
				return false;
			}
			return true;
		}
	}
}
