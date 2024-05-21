using SPT.Common.Http;
using SPT.Reflection.Patching;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.Quests
{
    /// <summary>
    /// Having the raid timer reach zero results in a successful extract,
    /// this patch makes it so letting the time reach zero results in a MIA result
    /// </summary>
    public class EndByTimerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseLocalGame<EftGamePlayerOwner>), nameof(BaseLocalGame<EftGamePlayerOwner>.Stop));
        }

        // Unused, but left here in case patch breaks and finding the intended method is difficult
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
        private static bool PrefixPatch(ref ExitStatus exitStatus, ref string exitName)
        {
            var isParsed = bool.TryParse(RequestHandler.GetJson("/singleplayer/settings/raid/endstate"), out bool MIAOnRaidEnd);
            if (isParsed)
            {
                // No extract name and successful, its a MIA
                if (MIAOnRaidEnd && string.IsNullOrEmpty(exitName?.Trim()) && exitStatus == ExitStatus.Survived)
                {
                    exitStatus = ExitStatus.MissingInAction;
                    exitName = null;
                }
            }
            return true; // Do original
        }
    }
}
