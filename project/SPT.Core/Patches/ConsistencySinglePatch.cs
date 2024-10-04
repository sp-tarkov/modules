﻿using System.Reflection;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using SPT.Core.Models;
using FilesChecker;

namespace SPT.Core.Patches
{
    public class ConsistencySinglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.FilesCheckerTypes.SingleCustom(x => x.Name == "ConsistencyController")
                .GetMethods().SingleCustom(x => x.Name == "EnsureConsistencySingle" && x.ReturnType == typeof(Task<ICheckResult>));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref object __result)
        {
            __result = Task.FromResult<ICheckResult>(new FakeFileCheckerResult());

            return false; // Skip original
        }
    }
}
