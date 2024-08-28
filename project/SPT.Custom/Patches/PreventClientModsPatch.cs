using System;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SPT.Custom.Utils;

namespace SPT.Custom.Patches
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
        public static void PatchPrefix()
        {
            CheckForNonWhitelistedPlugins(Logger);
        }

        private static void CheckForNonWhitelistedPlugins(ManualLogSource logger)
        {
            if (MenuNotificationManager.disallowedPlugins.Any())
            {
                logger.LogError($"{MenuNotificationManager.release.illegalPluginsLoadedText}\n{string.Join("\n", MenuNotificationManager.disallowedPlugins)}");
                throw new Exception(MenuNotificationManager.release.illegalPluginsExceptionText);
            }
        }
    }
}