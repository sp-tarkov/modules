using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Aki.Core.Models;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using FilesChecker;
using HarmonyLib;
using System;

namespace Aki.Core.Patches
{
    public class DataHandlerDebugPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes
                .Single(t => t.Name == "DataHandler")
                .GetMethod("method_5", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPrefix(ref string __result)
        {
            Console.WriteLine($"response json: ${__result}");
        }
    }
}