using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

public class QTEPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HideoutPlayerOwner), nameof(HideoutPlayerOwner.StopWorkout));
    }

    [PatchPostfix]
    public static void PatchPostfix(HideoutPlayerOwner __instance)
    {
        RequestHandler.PutJson(
            "/client/hideout/workout",
            new { skills = new SkillsDescriptor(__instance.HideoutPlayer.Skills) }.ToJson()
        );
    }
}
