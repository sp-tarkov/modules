using Aki.Common.Http;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Alter the max bot cap with value stored in server, if value is -1, use existing value
    /// </summary>
    class MaxBotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            const string methodName = "SetSettings";
            var desiredType = PatchConstants.EftTypes.Single(x => x.GetMethod(methodName, flags) != null && IsTargetMethod(x.GetMethod(methodName, flags)));
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
            var json = RequestHandler.GetJson("/singleplayer/settings/bot/maxCap");
            var isParsable = int.TryParse(json, out maxCount);

            if (isParsable)
            {
                if (maxCount == -1)
                {
                    return;
                }

                maxCount = isParsable
                    ? maxCount
                    : 20;
            }
        }
    }
}