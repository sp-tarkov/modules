using EFT;
using HarmonyLib;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private static bool PatchPrefix(ref bool __result, BotsGroup __instance, IPlayer requester)
        {
            if (requester == null)
            {
                __result = false;
                return false; // Skip original
            }

            // Check existing enemies list
            // Could also check x.Value.Player?.Id - BSG do it this way
            if (!__instance.Enemies.IsNullOrEmpty() && __instance.Enemies.Keys.ContainsPlayer(requester))
            {
                __result = true;
                return false; // Skip original
            }

            // Do not force bots to be enemies if they are allies
            if (!__instance.Allies.IsNullOrEmpty() && __instance.Allies.ContainsPlayer(requester))
            {
                __result = false;
                return false; // Skip original
            }

            // Bots should not become hostile with their group members here. This is needed in case mods add mixed groups (i.e. BEAR's and USEC's).
            if (__instance.GetAllMembers().ContainsPlayer(requester))
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
                || __instance.InitialBotType == WildSpawnType.ravangeZryachiyEvent
                || __instance.InitialBotType == WildSpawnType.bossZryachiy
                || __instance.InitialBotType == WildSpawnType.followerZryachiy)
            {
                return true; // Do original code
            }

            // Let EFT manage Rogue behavior toward PMC's
            if (__instance.InitialBotType == WildSpawnType.exUsec
                && __instance.Side == EPlayerSide.Savage
                && requester.Side != EPlayerSide.Savage)
            {
                return true; // Do original code
            }

            // In all other cases, requester needs to be added to the enemies collection of the bot group if it should be treated as hostile
            // NOTE: Manually adding enemies is needed as a result of EFT's implementation of PMC's because they are not hostile toward
            //       Scavs (any probably other bot types too)
            __result = CheckIfPlayerShouldBeEnemy(__instance, requester);
            if (__result)
            {
                __instance.AddEnemy(requester, EBotEnemyCause.checkAddTODO);
            }

            return false; // Skip original
        }

        /// <summary>
        /// Returns true if requester should be an enemy of the bot group
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="requester"></param>
        /// <returns></returns>
        private static bool CheckIfPlayerShouldBeEnemy(BotsGroup __instance, IPlayer requester)
        {
            switch (__instance.Side)
            {
                case EPlayerSide.Usec:
                    return requester.Side != EPlayerSide.Usec || ShouldAttackUsec(requester);
                case EPlayerSide.Bear:
                    return requester.Side != EPlayerSide.Bear || ShouldAttackBear(requester);
                case EPlayerSide.Savage:
                    return requester.Side != EPlayerSide.Savage;
            }

            return false;
        }

        /// <summary>
        /// Return True when usec default behavior is attack + bot is usec
        /// </summary>
        /// <param name="requester"></param>
        /// <returns></returns>
        private static bool ShouldAttackUsec(IPlayer requester)
        {
            var requesterMind = requester?.AIData?.BotOwner?.Settings?.FileSettings?.Mind;

            if (requesterMind == null)
            {
                return false;
            }

            return requester.IsAI && requesterMind.DEFAULT_USEC_BEHAVIOUR == EWarnBehaviour.Attack && requester.Side == EPlayerSide.Usec;
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

            return requester.IsAI && requesterMind.DEFAULT_BEAR_BEHAVIOUR == EWarnBehaviour.Attack && requester.Side == EPlayerSide.Bear;
        }
    }
}
