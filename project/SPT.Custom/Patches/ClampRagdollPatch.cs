using SPT.Reflection.Patching;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// On death some bots have a habit of flying upwards. We have found this occurs when the velocity y and magnitude match
    /// </summary>
    public class ClampRagdollPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Corpse), nameof(Corpse.method_16));
        }

        [PatchPrefix]
        private static void PatchPreFix(ref Vector3 velocity)
        {
            if (velocity.magnitude == velocity.y && velocity.y > 5)
            {
                // Probably flying upwards
                velocity.y = 1;
            }
        }
    }
}
