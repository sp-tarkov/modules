using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.HealthSystem;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Healing
{
    public class MainMenuControllerPatch : ModulePatch
    {
        static MainMenuControllerPatch()
        {
            _ = nameof(IHealthController.HydrationChangedEvent);
            _ = nameof(MainMenuController.HealthController);
        }

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MainMenuController);
            var desiredMethod = desiredType.GetMethod("method_1", PatchConstants.PrivateFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix(MainMenuController __instance)
        {
            var healthController = __instance.HealthController;
            var listener = Utils.Healing.HealthListener.Instance;

            if (healthController == null)
            {
                Logger.LogInfo("MainMenuControllerPatch() - healthController is null");
            }

            if (listener == null)
            {
                Logger.LogInfo("MainMenuControllerPatch() - listener is null");
            }

            if (healthController != null && listener != null)
            {
                listener.Init(healthController, false);
            }
                
        }
    }
}
