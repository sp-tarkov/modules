using SPT.Common.Http;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    public class QTEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutPlayerOwner), nameof(HideoutPlayerOwner.StopWorkout));
        }

        [PatchPostfix]
        public static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            RequestHandler.PutJson("/client/hideout/workout", new
            {
                skills = new GClass1985(__instance.HideoutPlayer.Skills)
            }
            .ToJson());
        }
    }
}
