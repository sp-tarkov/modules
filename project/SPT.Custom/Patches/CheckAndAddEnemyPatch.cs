using SPT.Reflection.Patching;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    public class CheckAndAddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.CheckAndAddEnemy));
        }

        /// <summary>
        /// CheckAndAddEnemy()
        /// Goal: This patch lets bosses shoot back once a PMC has shot them
        /// Removes the !player.AIData.IsAI  check
		/// BSG changed the way CheckAndAddEnemy Works in 14.0 Returns a bool now
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsGroup __instance, IPlayer player, ref bool __result)
		{
			// Set result to not include !player.AIData.IsAI checks
			__result = player.HealthController.IsAlive && !__instance.Enemies.ContainsKey(player) && __instance.AddEnemy(player, EBotEnemyCause.checkAddTODO);
			return false; // Skip Original
        }
    }
}
