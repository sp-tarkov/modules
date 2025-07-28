using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Alter the max bot cap with value stored in servers config/bot.json/maxBotCap, if value is -1, use existing value
    /// Adjusted value is set by client when 'botamount' is chosen in pre-raid dropdown,
    /// AsOnline = 20, Low = 15, Medium 20, High = 25, horde = 35
    /// Does not affect ALL bots, some bot types (e.g. bosses) are exempt
    /// </summary>
    public class OverrideMaxAiAliveInRaidValuePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.SetSettings));
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "maxCount"
                && parameters[1].Name == "botPresets"
                && parameters[2].Name == "botScatterings";
        }

        [PatchPrefix]
        public static void PatchPreFix(ref int maxCount)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var location = gameWorld.MainPlayer?.Location ?? "default";

            if (int.TryParse(RequestHandler.GetJson($"/singleplayer/settings/bot/maxCap/{location}"), out var parsedMaxCount))
            {
                Logger.LogInfo($"Set max bot cap for: {location} from: {maxCount} to: {parsedMaxCount}");
                maxCount = parsedMaxCount;
            }
            else
            {
                Logger.LogError($"Unable to parse data from singleplayer/settings/bot/maxCap, using existing: {location} max: {maxCount}");
            }
        }
    }
}
