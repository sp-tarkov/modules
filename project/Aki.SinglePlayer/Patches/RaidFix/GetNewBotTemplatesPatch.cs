using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFT;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.SinglePlayer.Models.RaidFix;
using System;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class GetNewBotTemplatesPatch : ModulePatch
    {
        private static MethodInfo _getNewProfileMethod;

        static GetNewBotTemplatesPatch()
        {
            _ = nameof(IBotData.PrepareToLoadBackend);
            _ = nameof(BotsPresets.GetNewProfile);
            _ = nameof(PoolManager.LoadBundlesAndCreatePools);
            _ = nameof(JobPriority.General);
        }

        public GetNewBotTemplatesPatch()
        {
            var desiredType = typeof(BotsPresets);
            _getNewProfileMethod = desiredType
                .GetMethod(nameof(BotsPresets.GetNewProfile), BindingFlags.Instance | BindingFlags.NonPublic); // want the func with 2 params (protected + inherited from base)

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {_getNewProfileMethod?.Name}");
        }

        /// <summary>
        /// Looking for CreateProfile()
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsPresets).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Single(x => IsTargetMethod(x));
        }

        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 3
                && parameters[0].Name == "data"
                && parameters[1].Name == "cancellationToken"
                && parameters[2].Name == "withDelete");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, GClass626 data, ref bool withDelete)
        {
            withDelete = true;

            return true; // do original method
        }

        private static Profile GetFirstResult(Task<Profile[]> task)
        {
            var result = task.Result[0];
            Logger.LogInfo($"{DateTime.Now:T} Loading bot {result.Info.Nickname} profile from server. role: {result.Info.Settings.Role} side: {result.Side}");

            return result;
        }
    }
}
