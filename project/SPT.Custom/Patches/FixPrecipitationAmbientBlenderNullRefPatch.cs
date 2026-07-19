using System.Reflection;
using Audio.AmbientSubsystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;
// TODO: Move to asm tool
public class FixPrecipitationAmbientBlenderNullRefPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PrecipitationAmbientBlender), nameof(PrecipitationAmbientBlender.SetClip));
    }

    [PatchPrefix]
    public static bool PatchPrefix(IAudioCrossfader ____crossfader, ref int ____lastClipHash, PrecipitationAmbientBlender __instance)
    {
        // Skip original as BSG added no null checks here
        if (____crossfader == null)
        {
            return false;
        }

        if (__instance.CurrentPrecipitationIntensity == RainController.ERainIntensity.None)
        {
            ____crossfader.MixSource.clip = null;
            ____lastClipHash = -1;
            return false;
        }

        if (__instance.TryGetClip(out var audioClip))
        {
            if(audioClip == null)
            {
                ____lastClipHash = -1;
                return false;
            }

            ____crossfader.MixSource.clip = audioClip;
            ____lastClipHash = audioClip.GetHashCode();
        }

        return false;
    }

}
