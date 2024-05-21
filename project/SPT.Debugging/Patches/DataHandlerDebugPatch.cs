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
            return AccessTools.Method(typeof(DataHandler), nameof(DataHandler.method_5));
        }

        [PatchPostfix]
        private static void PatchPrefix(ref string __result)
        {
            Console.WriteLine($"response json: ${__result}");
        }
    }
}