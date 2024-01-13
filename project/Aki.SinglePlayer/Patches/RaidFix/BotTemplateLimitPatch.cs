using Aki.Reflection.Patching;
using Aki.Common.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class BotTemplateLimitPatch : ModulePatch
    {
        static BotTemplateLimitPatch()
        {
            _ = nameof(BotsPresets.CreateProfile);
            _ = nameof(WaveInfo.Difficulty);
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsPresets), nameof(BotsPresets.method_1));
        }

        [PatchPostfix]
        private static void PatchPostfix(List<WaveInfo> __result, List<WaveInfo> wavesProfiles, List<WaveInfo> delayed)
        {
            /*
                Method sums Limits by grouping wavesPropfiles collection by Role and Difficulty
                then in each group sets Limit to 30, the remainder is stored in "delayed" collection.
                So we change Limit of each group.
                Clear delayed waves, we don't need them if we have enough loaded profiles and in method_2 it creates a lot of garbage.
            */

            delayed?.Clear();
            
            foreach (WaveInfo wave in __result)
            {
                var json = RequestHandler.GetJson($"/singleplayer/settings/bot/limit/{wave.Role}");
                wave.Limit = (string.IsNullOrWhiteSpace(json))
                    ? 30
                    : Convert.ToInt32(json);
            }
        }
    }
}
