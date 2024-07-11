using SPT.Reflection.Patching;
using System.Reflection;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    public class BotsGroupLetBossesShootPmcsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.CheckAndAddEnemy));
        }

        /// <summary>
        /// CheckAndAddEnemy()
        /// Goal: This patch lets bosses shoot back once a PMC has shot them
        /// Force method to always run the code
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(ref bool ignoreAI)
		{
            // original
            // return player.HealthController.IsAlive && (!player.AIData.IsAI || ignoreAI) && !this.Enemies.ContainsKey(player) && this.AddEnemy(player, EBotEnemyCause.checkAddTODO);
            ignoreAI = true;


            return true; // Do Original
        }
    }
}
