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
            _getNewProfileMethod = typeof(BotsPresets)
                .GetMethod(nameof(BotsPresets.GetNewProfile), PatchConstants.PrivateFlags);
        }

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
        private static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, IBotData data)
        {
            /*
                in short when client wants new bot and GetNewProfile() return null (if not more available templates or they don't satisfy by Role and Difficulty condition)
                then client gets new piece of WaveInfo collection (with Limit = 30 by default) and make request to server
                but use only first value in response (this creates a lot of garbage and cause freezes)
                after patch we request only 1 template from server

                along with other patches this one causes to call data.PrepareToLoadBackend(1) gets the result with required role and difficulty:
                new[] { new WaveInfo() { Limit = 1, Role = role, Difficulty = difficulty } }
                then perform request to server and get only first value of resulting single element collection
            */

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskAwaiter = (Task<Profile>)null;
            var profile = (Profile)_getNewProfileMethod.Invoke(__instance, new object[] { data });

            // load from server
            var source = data.PrepareToLoadBackend(1).ToList();
            taskAwaiter = PatchConstants.BackEndSession.LoadBots(source).ContinueWith(GetFirstResult, taskScheduler);

            // load bundles for bot profile
            var continuation = new BundleLoader(taskScheduler);
            __result = taskAwaiter.ContinueWith(continuation.LoadBundles, taskScheduler).Unwrap();

            return false;
        }

        private static Profile GetFirstResult(Task<Profile[]> task)
        {
            var result = task.Result[0];
            Logger.LogInfo($"{DateTime.Now:T} Loading bot {result.Info.Nickname} profile from server. role: {result.Info.Settings.Role} side: {result.Side}");

            return result;
        }
    }
}
