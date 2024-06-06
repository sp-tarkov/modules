using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.Custom.Patches
{
    public class PMCSpawnParamPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.GetGroupAndSetEnemies));
        }

        [PatchPrefix]
        private static void PatchPrefix(BotOwner bot)
        {
            if (bot.SpawnProfileData.SpawnParams != null
                && (bot.Profile.Info.Settings.Role != WildSpawnType.pmcBEAR || bot.Profile.Info.Settings.Role != WildSpawnType.pmcUSEC))
            {
                return;
            }

            // Add SpawnParams to PMC bots that are missing them
            bot.SpawnProfileData.SpawnParams = new BotSpawnParams();
            bot.SpawnProfileData.SpawnParams.ShallBeGroup = new ShallBeGroupParams(false, false);
        }
    }
}
