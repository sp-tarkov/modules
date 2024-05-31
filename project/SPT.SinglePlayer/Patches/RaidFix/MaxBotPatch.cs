using Comfort.Common;
using EFT;
using SPT.Common.Http;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Alter the max bot cap with value stored in server, if value is -1, use existing value
    /// </summary>
    public class MaxBotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            const string methodName = "SetSettings";
            var desiredType = PatchConstants.EftTypes.SingleCustom(x => x.GetMethod(methodName, flags) != null && IsTargetMethod(x.GetMethod(methodName, flags)));
            var desiredMethod = desiredType.GetMethod(methodName, flags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
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
        private static void PatchPreFix(ref int maxCount)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var location = gameWorld.MainPlayer.Location;

            if (int.TryParse(RequestHandler.GetJson($"/singleplayer/settings/bot/maxCap/{location ?? "default"}"), out int parsedMaxCount))
            {
                Logger.LogWarning($"Set max bot cap to: {parsedMaxCount}");
                maxCount = parsedMaxCount;
            }
            else
            {
                Logger.LogWarning($"Unable to parse data from singleplayer/settings/bot/maxCap, using existing map max of {maxCount}");
            }
        }
    }
}