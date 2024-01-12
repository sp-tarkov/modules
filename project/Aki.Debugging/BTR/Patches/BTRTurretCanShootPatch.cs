using Aki.Reflection.Patching;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRTurretCanShootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRTurretServer), "method_1");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BTRTurretServer __instance)
        {
            Transform defaultTargetTransform = (Transform)AccessTools.Field(__instance.GetType(), "defaultTargetTransform").GetValue(__instance);

            bool flag = __instance.targetTransform != null && __instance.targetTransform != defaultTargetTransform;
            bool flag2 = (bool)AccessTools.Method(__instance.GetType(), "method_2").Invoke(__instance, null);
            bool flag3 = __instance.targetPosition != __instance.defaultAimingPosition;

            var isCanShootProperty = AccessTools.DeclaredProperty(__instance.GetType(), nameof(__instance.IsCanShoot));
            isCanShootProperty.SetValue(__instance, (flag || flag3) && flag2);

            return false;
        }
    }
}
