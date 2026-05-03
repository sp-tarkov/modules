using System.Reflection;
using Audio.AmbientSubsystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

public class FixPrecipitationAmbientBlenderNullRefPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PrecipitationAmbientBlender), nameof(PrecipitationAmbientBlender.method_4));
    }

    [PatchPrefix]
    public static bool PatchPrefix(IAudioCrossfader ___ginterface100_0, ref int ___int_0, PrecipitationAmbientBlender __instance)
    {
        // Skip original as BSG added no null checks here
        if (___ginterface100_0 == null)
        {
            return false;
        }

        if (__instance.CurrentPrecipitationIntensity == RainController.ERainIntensity.None)
        {
            ___ginterface100_0.MixSource.clip = null;
            ___int_0 = -1;
            return false;
        }

        if (__instance.TryGetClip(out var audioClip))
        {
            if(audioClip == null)
            {
                ___int_0 = -1;
                return false;
            }

            ___ginterface100_0.MixSource.clip = audioClip;
            ___int_0 = audioClip.GetHashCode();
        }

        return false;
    }

}
