using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Aki.Custom.BTR.Patches
{
    public class BTRDestroyAtRaidEndPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseLocalGame<GamePlayerOwner>), nameof(BaseLocalGame<GamePlayerOwner>.Stop));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                return;
            }

            var btrManager = gameWorld.GetComponent<BTRManager>();
            if (btrManager != null)
            {
                Logger.LogWarning("[AKI-BTR] BTRDestroyAtRaidEndPatch - Raid Ended: Destroying BTRManager");
                Object.Destroy(btrManager);
            }
        }
    }
}
