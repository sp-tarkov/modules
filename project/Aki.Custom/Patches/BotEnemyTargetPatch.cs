using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    public class BotEnemyTargetPatch : ModulePatch
    {
        private static Type _targetType;
        private static readonly string methodName = "AddEnemyToAllGroupsInBotZone";

        public BotEnemyTargetPatch()
        {
            _targetType = PatchConstants.EftTypes.Single(IsTargetType);
        }

        private bool IsTargetType(Type type)
        {
            if (type.Name == nameof(BotControllerClass) && type.GetMethod(methodName) != null)
            {
                Logger.LogInfo($"{methodName}: {type.FullName}");
                return true;
            }

            return false;
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod(methodName);
        }

        /// <summary>
        /// AddEnemyToAllGroupsInBotZone()
        /// Goal: by default, AddEnemyToAllGroupsInBotZone doesn't check if the bot group is on the same side as the player.
        /// The effect of this is that when you are a Scav and kill a Usec, every bot group in the zone will aggro you including other Scavs.
        /// This should fix that.
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotControllerClass __instance, IAIDetails aggressor, IAIDetails groupOwner, IAIDetails target)
        {
            BotZone botZone = groupOwner.AIData.BotOwner.BotsGroup.BotZone;
            foreach (var item in __instance.Groups())
            {
                if (item.Key != botZone)
                {
                    continue;
                }

                foreach (var group in item.Value.GetGroups(notNull: true))
                {
                    if (!group.Enemies.ContainsKey(aggressor) && ShouldAttack(aggressor, target, group))
                    {
                        group.AddEnemy(aggressor);
                    }
                }
            }

            return false;
        }
        private static bool ShouldAttack(IAIDetails attacker, IAIDetails victim, BotGroupClass groupToCheck)
        {
            // Group should target if player attack a victim on the same side or if the group is not on the same side as the player.
            bool shouldAttack = attacker.Side != groupToCheck.Side
                                || attacker.Side == victim.Side;

            return !groupToCheck.HaveMemberWithRole(WildSpawnType.gifter) && groupToCheck.ShallRevengeFor(victim) && shouldAttack;
        }
    }
}
