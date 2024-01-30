using Aki.Reflection.Patching;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.BTR.Patches
{
    public class BTRVehicleMovementSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRVehicle), nameof(BTRVehicle.Update));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref float ___float_10, float ___moveSpeed)
        {
            ___float_10 = ___moveSpeed * Time.deltaTime;
        }
    }
}
