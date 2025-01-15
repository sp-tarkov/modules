using System;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;

namespace SPT.Debugging.Patches
{
    public class DataHandlerDebugPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(DataHandlerClass), nameof(DataHandlerClass.method_5));
        }

        [PatchPostfix]
        public static void PatchPrefix(ref string __result)
        {
            Console.WriteLine($"response json: ${__result}");
        }
    }
}