using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;

namespace Aki.Core.Patches
{
    /// <summary>
    /// Prevents loading of non-whitelisted client mods to minimize the amount of false issue reports being made during the public BE phase
    /// </summary>
    public class PreventClientModsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.method_20));
        }

        [PatchPrefix]
        private static void Prefix()
        {
            AkiCorePlugin.CheckForNonWhitelistedPlugins();
        }
    }
}