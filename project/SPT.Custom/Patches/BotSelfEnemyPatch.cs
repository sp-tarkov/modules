using SPT.Reflection.Patching;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Goal: patch removes the current bot from its own enemy list - occurs when adding bots type to its enemy array in difficulty settings
    /// </summary>
    internal class BotSelfEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.PreActivate));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner __instance, BotsGroup group)
        {
            IPlayer selfToRemove = null;

            foreach (var enemy in group.Enemies)
            {
                if (enemy.Key.Id == __instance.Id)
                {
                    selfToRemove = enemy.Key;
                    break;
                }
            }

            if (selfToRemove != null)
            {
                group.Enemies.Remove(selfToRemove);
            }

            return true;
        }
    }
}
