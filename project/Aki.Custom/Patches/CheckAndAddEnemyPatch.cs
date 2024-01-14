using Aki.Reflection.Patching;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace Aki.Custom.Patches
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
        /// removes the !player.AIData.IsAI  check
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsGroup __instance, IPlayer player, ref bool ignoreAI)
        {
            // Z already has player as enemy BUT Enemies dict is empty, adding them again causes 'existing key' errors
            if (__instance.InitialBotType == WildSpawnType.bossZryachiy || __instance.InitialBotType == WildSpawnType.followerZryachiy)
            {
                return false;
            }

            if (!player.HealthController.IsAlive)
            {
                return false; // Skip original
            }

            if (!__instance.Enemies.ContainsKey(player))
            {
                __instance.AddEnemy(player, EBotEnemyCause.checkAddTODO);
            }            

            return false; // Skip original
        }
    }
}
