using Aki.Reflection.Patching;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace Aki.Custom.Patches
{
    public class BotEnemyTargetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.AddEnemyToAllGroupsInBotZone));
        }

        /// <summary>
        /// AddEnemyToAllGroupsInBotZone()
        /// Goal: by default, AddEnemyToAllGroupsInBotZone doesn't check if the bot group is on the same side as the player.
        /// The effect of this is that when you are a Scav and kill a Usec, every bot group in the zone will aggro you including other Scavs.
        /// This should fix that.
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsController __instance, IPlayer aggressor, IPlayer groupOwner, IPlayer target)
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
                        group.AddEnemy(aggressor, EBotEnemyCause.AddEnemyToAllGroupsInBotZone);
                    }
                }
            }

            return false;
        }
        private static bool ShouldAttack(IPlayer attacker, IPlayer victim, BotsGroup groupToCheck)
        {
            // Group should target if player attack a victim on the same side or if the group is not on the same side as the player.
            bool shouldAttack = attacker.Side != groupToCheck.Side
                                || attacker.Side == victim.Side;

            return !groupToCheck.HaveMemberWithRole(WildSpawnType.gifter) && groupToCheck.ShallRevengeFor(victim) && shouldAttack;
        }
    }
}
