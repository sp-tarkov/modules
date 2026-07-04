using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Core.Patches;

internal class Patch4001 : ModulePatch
{
    //HWEcho doesn't exist in the hollowed but does exist in the client, wtf?
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method("HWEcho:GetMetrics");
    }

    [PatchPrefix]
    public static bool Prefix(ref ValueTuple<string, string> __result)
    {
        __result = new();
        return false;
    }
}
