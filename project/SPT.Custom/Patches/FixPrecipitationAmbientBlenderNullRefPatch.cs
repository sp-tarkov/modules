using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Audio.AmbientSubsystem;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

public class FixPrecipitationAmbientBlenderNullRefPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PrecipitationAmbientBlender), nameof(PrecipitationAmbientBlender.method_4));
    }

    [PatchPrefix]
    public static bool PatchPrefix(GInterface100 ___ginterface100_0, ref int ___int_0, PrecipitationAmbientBlender __instance)
    {
        // Skip original as BSG added no null checks here
        if (___ginterface100_0 == null)
        {
            return false;
        }

        if (__instance.ERainIntensity_0 == RainController.ERainIntensity.None)
        {
            ___ginterface100_0.MixSource.clip = null;
            ___int_0 = -1;
            return false;
        }

        if(__instance.method_5(out var audioClip))
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
