using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace SPT.Custom.BTR.Patches
{
    // The BTRManager MapPathsConfiguration loading depends on the game state being set to Starting
    // so set it to Starting while the method is running, then reset it afterwards
    public class BTRPathLoadPatch : ModulePatch
    {
        private static PropertyInfo _statusProperty;
        private static GameStatus originalStatus;
        protected override MethodBase GetTargetMethod()
        {
            _statusProperty = AccessTools.Property(typeof(AbstractGame), nameof(AbstractGame.Status));

            return AccessTools.Method(typeof(BTRControllerClass), nameof(BTRControllerClass.method_1));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            originalStatus = Singleton<AbstractGame>.Instance.Status;
            _statusProperty.SetValue(Singleton<AbstractGame>.Instance, GameStatus.Starting);
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            _statusProperty.SetValue(Singleton<AbstractGame>.Instance, originalStatus);
        }
    }
}
