using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRIsDoorsClosedPath : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(VehicleBase), "IsDoorsClosed");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            var serverSideBTR = Singleton<GameWorld>.Instance?.BtrController.BtrVehicle;
            if (serverSideBTR == null)
            {
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
