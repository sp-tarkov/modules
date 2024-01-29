using Aki.Reflection.Patching;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.BTR.Patches
{
    /**
     * This class contains two patches because it's the two places that Application.targetFrameRate are used
     * and I wanted to keep the patches together
     * 
     * These patches are used to set the target framerate used by vehicle movement calculations to a static
     * value, avoiding issues caused by enabling NVidia Reflex. These values are then set back after the methods
     * complete
     */
    public static class BTRAppFrameratePatches
    {
        public class VehicleBaseInitFpsPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(VehicleBase), nameof(VehicleBase.Initialization));
            }

            [PatchPrefix]
            private static void PrefixPatch(out int __state)
            {
                __state = Application.targetFrameRate;
                Application.targetFrameRate = 60;
            }

            [PatchPostfix]
            private static void PostfixPatch(int __state)
            {
                Application.targetFrameRate = __state;
            }
        }

        public class VehicleBaseResetFpsPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(VehicleBase), nameof(VehicleBase.Reset));
            }

            [PatchPrefix]
            private static void PrefixPatch(out int __state)
            {
                __state = Application.targetFrameRate;
                Application.targetFrameRate = 60;
            }

            [PatchPostfix]
            private static void PostfixPatch(int __state)
            {
                Application.targetFrameRate = __state;
            }
        }
    }
}
