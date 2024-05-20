using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Aki.Custom.BTR.Patches
{
    public class BTRIsDoorsClosedPath : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(VehicleBase), nameof(VehicleBase.IsDoorsClosed));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError("[SPT-BTR] BTRIsDoorsClosedPatch - GameWorld is null");
                return true;
            }

            var serverSideBTR = gameWorld.BtrController.BtrVehicle;
            if (serverSideBTR == null)
            {
                Logger.LogError("[SPT-BTR] BTRIsDoorsClosedPatch - serverSideBTR is null");
                return true;
            }

            if (serverSideBTR.LeftSideState == 0 && serverSideBTR.RightSideState == 0)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
