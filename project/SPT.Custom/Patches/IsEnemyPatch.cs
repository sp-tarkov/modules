﻿using SPT.Reflection.Patching;
using EFT;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    public class IsEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsEnemy));
        }

        /// <summary>
        /// IsEnemy()
        /// Goal: Make bots take Side into account when deciding if another player/bot is an enemy
        /// Check enemy cache list first, if not found, check side, if they differ, add to enemy list and return true
        /// Needed to ensure bot checks the enemy side, not just its botType
        /// </summary>
        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result, BotsGroup __instance, IPlayer player)
        {
            if (player == null)
            {
                __result = false;

                return false; // Skip original
            }

            if (__instance.InitialBotType == WildSpawnType.peacefullZryachiyEvent
				|| __instance.InitialBotType == WildSpawnType.shooterBTR
				|| __instance.InitialBotType == WildSpawnType.gifter
				|| __instance.InitialBotType == WildSpawnType.sectantWarrior
				|| __instance.InitialBotType == WildSpawnType.sectantPriest
				|| __instance.InitialBotType == WildSpawnType.sectactPriestEvent
				|| __instance.InitialBotType == WildSpawnType.ravangeZryachiyEvent)
            {
                return true; // Do original code
            }

            var isEnemy = false; // default not an enemy

            // Check existing enemies list
            // Could also check x.Value.Player?.Id - BSG do it this way
            if (!__instance.Enemies.IsNullOrEmpty() && __instance.Enemies.Any(x => x.Key.Id == player.Id))
            {
                __result = true;
                return false; // Skip original
            }

            // Weird edge case - without this you get spammed with key already in enemy list error when you move around on lighthouse
            // Make zryachiy use existing isEnemy() code
            if (__instance.InitialBotType == WildSpawnType.bossZryachiy)
            {
                return false; // Skip original
            }

            if (__instance.Side == EPlayerSide.Usec)
            {
                if (player.Side == EPlayerSide.Bear || player.Side == EPlayerSide.Savage ||
                    ShouldAttackUsec(player))
                {
                    isEnemy = true;
                    __instance.AddEnemy(player, EBotEnemyCause.checkAddTODO);
                }
            }
            else if (__instance.Side == EPlayerSide.Bear)
            {
                if (player.Side == EPlayerSide.Usec || player.Side == EPlayerSide.Savage ||
                    ShouldAttackBear(player))
                {
                    isEnemy = true;
                    __instance.AddEnemy(player, EBotEnemyCause.checkAddTODO);
                }
            }
            else if (__instance.Side == EPlayerSide.Savage)
            {
                if (player.Side != EPlayerSide.Savage)
                {
                    //Lets exUsec warn Usecs and fire at will at Bears
                    if (__instance.InitialBotType == WildSpawnType.exUsec)
                    {
                        return true; // Let BSG handle things
                    }
                    // everyone else is an enemy to savage (scavs)
                    isEnemy = true;
                    __instance.AddEnemy(player, EBotEnemyCause.checkAddTODO);
                }
            }

            __result = isEnemy;

            return false; // Skip original
        }

        /// <summary>
        /// Return True when usec default behavior is attack + bot is usec
        /// </summary>
        /// <param name="requester"></param>
        /// <returns>bool</returns>
        private static bool ShouldAttackUsec(IPlayer requester)
        {
            var requesterMind = requester?.AIData?.BotOwner?.Settings?.FileSettings?.Mind;

            if (requesterMind == null)
            {
                return false;
            }

            return requester.IsAI && requesterMind.DEFAULT_USEC_BEHAVIOUR == EWarnBehaviour.AlwaysEnemies && requester.Side == EPlayerSide.Usec;
        }

        /// <summary>
        /// Return True when bear default behavior is attack + bot is bear
        /// </summary>
        /// <param name="requester"></param>
        /// <returns></returns>
        private static bool ShouldAttackBear(IPlayer requester)
        {
            var requesterMind = requester.AIData?.BotOwner?.Settings?.FileSettings?.Mind;

            if (requesterMind == null)
            {
                return false;
            }

            return requester.IsAI && requesterMind.DEFAULT_BEAR_BEHAVIOUR == EWarnBehaviour.AlwaysEnemies && requester.Side == EPlayerSide.Bear;
        }
    }
}
