using Aki.Reflection.Patching;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.BTR.Patches
{
    public class BTRTurretCanShootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRTurretServer), nameof(BTRTurretServer.method_1));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BTRTurretServer __instance, Transform ___defaultTargetTransform)
        {
            bool flag = __instance.targetTransform != null && __instance.targetTransform != ___defaultTargetTransform;
            bool flag2 = __instance.method_2();
            bool flag3 = __instance.targetPosition != __instance.defaultAimingPosition;

            var isCanShootProperty = AccessTools.DeclaredProperty(__instance.GetType(), nameof(__instance.IsCanShoot));
            isCanShootProperty.SetValue(__instance, (flag || flag3) && flag2);

            return false;
        }
    }
}
