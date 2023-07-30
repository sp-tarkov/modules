using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    public class CheckAndAddEnemyPatch : ModulePatch
    {
        private static Type _targetType;
        private readonly string _targetMethodName = "CheckAndAddEnemy";

        /// <summary>
        /// BotGroupClass.CheckAndAddEnemy()
        /// </summary>
        public CheckAndAddEnemyPatch()
        {
            _targetType = PatchConstants.EftTypes.Single(IsTargetType);
        }

        private bool IsTargetType(Type type)
        {
            if (type.GetMethod("AddEnemy") != null && type.GetMethod("AddEnemyGroupIfAllowed") != null)
            {
                return true;
            }

            return false;
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod(_targetMethodName);
        }

        /// <summary>
        /// CheckAndAddEnemy()
        /// Goal: This patch lets bosses shoot back once a PMC has shot them
        /// removes the !player.AIData.IsAI  check
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotGroupClass __instance, IAIDetails player, ref bool ignoreAI)
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
                __instance.AddEnemy(player);
            }            

            return false; // Skip original
        }
    }
}
