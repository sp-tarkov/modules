using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    public class CheckAndAddEnemyPatch : ModulePatch
    {
        private static Type _targetType;
        private static FieldInfo _sideField;
        private static FieldInfo _enemiesField;
        private static FieldInfo _spawnTypeField;
        private static MethodInfo _addEnemy;
        private readonly string _targetMethodName = "CheckAndAddEnemy";

        /// <summary>
        /// BotGroupClass.CheckAndAddEnemy()
        /// </summary>
        public CheckAndAddEnemyPatch()
        {
            _targetType = PatchConstants.EftTypes.Single(IsTargetType);
            _sideField = _targetType.GetField("Side");
            _enemiesField = _targetType.GetField("Enemies");
            _spawnTypeField = _targetType.GetField("wildSpawnType_0", BindingFlags.NonPublic | BindingFlags.Instance);
            _addEnemy = _targetType.GetMethod("AddEnemy");
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
        private static bool PatchPrefix(BotGroupClass __instance, IAIDetails player, ref bool ignoreAI, Dictionary<IAIDetails, BotSettingsClass> ___Enemies)
        {
            //var side = (EPlayerSide)_sideField.GetValue(__instance);
            //var botType = (WildSpawnType)_spawnTypeField.GetValue(__instance);

            if (!player.HealthController.IsAlive)
            {
                return false; // do nothing and skip
            }

            if (!___Enemies.ContainsKey(player))
            {
                __instance.AddEnemy(player);
            }

            // Add enemy to list
            //if (!enemies.ContainsKey(player) && (!playerIsAi || ignoreAI))
            //_addEnemy.Invoke(__instance, new IAIDetails[] { player });
            

            return false;
        }
    }
}
