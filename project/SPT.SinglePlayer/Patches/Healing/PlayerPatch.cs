using System;
using SPT.Reflection.Patching;
using EFT;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.Healing
{
    public class PlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Init));
        }

        [PatchPostfix]
        private static async void PatchPostfix(Task __result, Player __instance, Profile profile)
        {
            await __result;

            if (profile?.Id.Equals(Common.Http.RequestHandler.SessionId, StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Logger.LogDebug($"Hooking up health listener to profile: {profile.Id}");
                var listener = Utils.Healing.HealthListener.Instance;
                listener.Init(__instance.HealthController, true);
                Logger.LogDebug($"HealthController instance: {__instance.HealthController.GetHashCode()}");
            }
            else
            {
                Logger.LogDebug($"Skipped on HealthController instance: {__instance.HealthController.GetHashCode()} for profile id: {profile?.Id}");
            }
        }
    }
}
