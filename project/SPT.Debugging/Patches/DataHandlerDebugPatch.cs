using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Debugging.Patches;

public class DataHandlerDebugPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HTTPTransportManager), nameof(HTTPTransportManager.ParseResponseToText));
    }

    [PatchPostfix]
    public static void PatchPrefix(ref string __result)
    {
        Console.WriteLine($"response json: ${__result}");
    }
}
