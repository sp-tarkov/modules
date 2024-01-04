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
            var btrView = Singleton<GameWorld>.Instance?.BtrController?.BtrView;
            if (btrView == null)
            {
                return true;
            }

            var btrSides = (BTRSide[])AccessTools.Field(typeof(BTRView), "_btrSides").GetValue(btrView);
            int doorsClosed = 0;
            foreach (var side in btrSides)
            {
                if (side.State == BTRSide.EState.Free)
                {
                    doorsClosed++;
                }
            }
            if (doorsClosed == btrSides.Length)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
