using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /**
     * It's possible for `AddEnemy` to return false, in that case, further code in TryCall will fail,
     * so we do the first bit of `TryCall` ourselves, and skip the original function if AddEnemy fails
     */
    internal class BotCalledDataTryCallPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotCalledData), nameof(BotCalledData.TryCall));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, BotOwner caller, BotOwner ___botOwner_0, BotOwner ____caller)
        {
            if (___botOwner_0.EnemiesController.IsEnemy(caller.AIData.Player) || ____caller != null)
            {
                __result = false;

                // Skip original
                return false;
            }

            if (caller.Memory.GoalEnemy != null)
            {
                IPlayer person = caller.Memory.GoalEnemy.Person;
                if (!___botOwner_0.BotsGroup.Enemies.ContainsKey(person))
                {
                    if (!___botOwner_0.BotsGroup.AddEnemy(person, EBotEnemyCause.callBot))
                    {
                        __result = false;

                        // Skip original
                        return false;
                    }
                }
            }

            // Allow original
            return true;
        }
    }
}
