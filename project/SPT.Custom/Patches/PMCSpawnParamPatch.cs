﻿using EFT;
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
            // is a boss and not a follower and not a PMC
            if (!bot.Profile.Info.Settings.IsBoss() && !CustomAI.AiHelpers.BotIsSptPmc(bot.Profile.Info.Settings.Role, bot))
            {
                return;
            }                 
            // is a boss and follower and a pmc
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
