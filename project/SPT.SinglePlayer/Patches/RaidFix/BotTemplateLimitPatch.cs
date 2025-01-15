using SPT.Reflection.Patching;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    public class BotTemplateLimitPatch : ModulePatch
    {
        static BotTemplateLimitPatch()
        {
            _ = nameof(BotsPresets.CreateProfile);
            _ = nameof(WaveInfoClass.Difficulty);
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsPresets), nameof(BotsPresets.method_3));
        }

        [PatchPostfix]
        public static void PatchPostfix(List<WaveInfoClass> __result, List<WaveInfoClass> wavesProfiles, List<WaveInfoClass> delayed)
        {
            /*
                Method sums Limits by grouping wavesPropfiles collection by Role and Difficulty
                then in each group sets Limit to 30, the remainder is stored in "delayed" collection.
                So we change Limit of each group.
                Clear delayed waves, we don't need them if we have enough loaded profiles and in method_2 it creates a lot of garbage.
            */

            delayed?.Clear();
            
            foreach (WaveInfoClass wave in __result)
            {
                var json = RequestHandler.GetJson($"/singleplayer/settings/bot/limit/{wave.Role}");
                wave.Limit = (string.IsNullOrWhiteSpace(json))
                    ? 30
                    : Convert.ToInt32(json);
            }
        }
    }
}
