using Aki.Reflection.Patching;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.Patches
{
    public class ClampRagdollPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Corpse), nameof(Corpse.method_16));
        }

        [PatchPrefix]
        private static void PatchPreFix(ref Vector3 velocity)
        {
            velocity.y = Mathf.Clamp(velocity.y, -1f, 1f);
        }
    }
}
