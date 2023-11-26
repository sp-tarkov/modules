using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.ScavMode
{
    /// <summary>
    /// Just before converting the `EscapeTimeLimit` value from _raidsettings.SelectedLocation.EscapeTimeLimit
    /// into a TimeSpam, call into server and get a different value
    /// If -1 is passed in return the original value
    /// </summary>
    public class ScavLateStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "TarkovApplication");
            var desiredMethod = Array.Find(desiredType.GetMethods(PatchConstants.PrivateFlags), IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private bool IsTargetMethod(MethodInfo arg)
        {
            // method_44 as of 26535
            var parameters = arg.GetParameters();
            return parameters.Length == 1
                && parameters[0]?.Name == "defaultMinutes"
                && arg.ReturnType == typeof(TimeSpan);
        }

        [PatchPrefix]
        private static bool PatchPrefix(int defaultMinutes, ref TimeSpan __result, RaidSettings ____raidSettings)
        {
            var request = new RaidTimeRequest(____raidSettings.Side, ____raidSettings.SelectedLocation.Id);
            var json = RequestHandler.PostJson("/singleplayer/settings/getRaidTime", Json.Serialize(request));
            var serverResult = Json.Deserialize<RaidTimeResponse>(json);

            if (serverResult.RaidTimeMinutes == -1)
            {
                // Default value passed in, make no changes
                return true;
            }

            __result = TimeSpan.FromSeconds(60 * serverResult.RaidTimeMinutes);
            return false; // Skip original
        }

        public class RaidTimeResponse
        {
            public int RaidTimeMinutes { get; set; }
        }

        public class RaidTimeRequest
        {
            public RaidTimeRequest(ESideType side, string location)
            {
                Side = side;
                Location = location;
            }

            public ESideType Side { get; set; }
            public string Location { get; set; }
        }
    }
}
