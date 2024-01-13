using Aki.Common.Http;
using Aki.Reflection.Patching;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace Aki.Custom.Patches
{
    public class QTEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutPlayerOwner), nameof(HideoutPlayerOwner.StopWorkout));
        }

        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            RequestHandler.PutJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }
            .ToJson());
        }
    }
}
