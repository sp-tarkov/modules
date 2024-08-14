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

            // Do not force bots to be enemies if they are allies
            // Note: This works because BotsGroup::AddAlly has a Player parameter despite the Allies list being IPlayers. If this ever changes,
            //       the ID's will need to be compared because the object references may not match.
            if (__instance.Allies.Contains(requester))
            {
                __result = false;
                return false; // Skip original
            }

            // Check existing enemies list
            // Could also check x.Value.Player?.Id - BSG do it this way
            if (!__instance.Enemies.IsNullOrEmpty() && __instance.Enemies.Any(x => x.Key.Id == requester.Id))
            {
                __result = true;
                return false; // Skip original
            }

            // Weird edge case - without this you get spammed with key already in enemy list error when you move around on lighthouse
            // Make zryachiy use existing isEnemy() code
            if (__instance.InitialBotType == WildSpawnType.bossZryachiy)
            {
                // TODO: Should __result be set here, or should this actaully return true?
                return false; // Skip original
            }

            // Bots should not become hostile with their group members here. This is needed in case mods add mixed groups (i.e. BEAR's and USEC's).
            if (__instance.GetAllMembers().Any(i => i.Id == requester.Id))
            {
                __result = false;

                return false; // Skip original
            }

            var isEnemy = false; // default not an enemy

            if (__instance.Side == EPlayerSide.Usec)
            {
                if (requester.Side == EPlayerSide.Bear || requester.Side == EPlayerSide.Savage ||
                    ShouldAttackUsec(requester))
                {
                    isEnemy = true;
                    __instance.AddEnemy(requester, EBotEnemyCause.checkAddTODO);
                }
            }
            else if (__instance.Side == EPlayerSide.Bear)
            {
                if (requester.Side == EPlayerSide.Usec || requester.Side == EPlayerSide.Savage ||
                    ShouldAttackBear(requester))
                {
                    isEnemy = true;
                    __instance.AddEnemy(requester, EBotEnemyCause.checkAddTODO);
                }
            }
            else if (__instance.Side == EPlayerSide.Savage)
            {
                if (requester.Side != EPlayerSide.Savage)
                {
                    //Lets exUsec warn Usecs and fire at will at Bears
                    if (__instance.InitialBotType == WildSpawnType.exUsec)
                    {
                        return true; // Let BSG handle things
                    }
                    // everyone else is an enemy to savage (scavs)
                    isEnemy = true;
                    __instance.AddEnemy(requester, EBotEnemyCause.checkAddTODO);
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
