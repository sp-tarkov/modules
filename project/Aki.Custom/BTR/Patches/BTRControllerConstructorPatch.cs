using Aki.Reflection.Patching;
using HarmonyLib;
using System.Reflection;

namespace Aki.Custom.BTR.Patches
{
    public class BTRControllerConstructorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredConstructors(typeof(BTRControllerClass))[0];
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false; // We don't want the original constructor to run
        }
    }
}
