using SPT.Reflection.Patching;
using EFT.HealthSystem;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.Healing
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
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_1));
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
