using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Quests
{
    /// <summary>
    /// Having the raid timer reach zero results in a successful extract,
    /// this patch makes it so letting the time reach zero results in a MIA result
    /// </summary>
    public class EndByTimerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.LocalGameType.BaseType;
            var desiredMethod = desiredType.GetMethods(PatchConstants.PrivateFlags).SingleOrDefault(IsStopRaidMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsStopRaidMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 4
                && parameters[0].Name == "profileId"
                && parameters[1].Name == "exitStatus"
                && parameters[2].Name == "exitName"
                && parameters[3].Name == "delay"
                && parameters[0].ParameterType == typeof(string)
                && parameters[1].ParameterType == typeof(ExitStatus)
                && parameters[2].ParameterType == typeof(string)
                && parameters[3].ParameterType == typeof(float));
        }

        [PatchPrefix]
        private static bool PrefixPatch(object __instance, ref ExitStatus exitStatus, ref string exitName)
        {
            var isParsed = bool.TryParse(RequestHandler.GetJson("/singleplayer/settings/raid/endstate"), out bool MIAOnRaidEnd);
            if (isParsed)
            {
                // No extract name and successful, its a MIA
                if (MIAOnRaidEnd == true && string.IsNullOrEmpty(exitName?.Trim()) && exitStatus == ExitStatus.Survived)
                {
                    exitStatus = ExitStatus.MissingInAction;
                    exitName = null;
                }
            }
            return true; // Do original
        }
    }
}
