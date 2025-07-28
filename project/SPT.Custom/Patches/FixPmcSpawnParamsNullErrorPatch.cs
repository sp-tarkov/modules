using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches
{
    public class FixPmcSpawnParamsNullErrorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.GetGroupAndSetEnemies));
        }

        [PatchPrefix]
        public static void PatchPrefix(BotOwner bot)
        {
            // Is a boss and not a follower and not a PMC
            if (!bot.Profile.Info.Settings.IsBoss() && !bot.IsPMC())
            {
                return;
            }

            // Is a boss and follower and a pmc - nullguard SpawnParams property
            if (bot.SpawnProfileData.SpawnParams == null)
            {
                bot.SpawnProfileData.SpawnParams = new BotSpawnParams();
            }

            if (bot.SpawnProfileData.SpawnParams.ShallBeGroup == null)
            {
                bot.SpawnProfileData.SpawnParams.ShallBeGroup = new ShallBeGroupParams(false, false);
            }
        }
    }
}
