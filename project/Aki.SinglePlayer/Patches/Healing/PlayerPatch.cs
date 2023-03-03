using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System.Reflection;
using System.Threading.Tasks;

namespace Aki.SinglePlayer.Patches.Healing
{
    public class PlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(Player);
            var desiredMethod = desiredType.GetMethod("Init", PatchConstants.PrivateFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        private static async void PatchPostfix(Task __result, Player __instance, Profile profile)
        {
            await __result;

            if (profile?.Id.StartsWith("pmc") == true)
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
