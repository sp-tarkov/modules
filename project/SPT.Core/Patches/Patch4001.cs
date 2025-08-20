﻿using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Core.Patches;

internal class Patch4001 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass914), nameof(GClass914.GetMetrics));
    }

    [PatchPrefix]
    public static bool Prefix(ref ValueTuple<string, string> __result)
    {
        __result = new();
        return false;
    }
}
