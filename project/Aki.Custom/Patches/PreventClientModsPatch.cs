using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.MainMenu
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
            CheckForNonWhitelistedPlugins(Logger);
        }

        private static void CheckForNonWhitelistedPlugins(ManualLogSource logger)
        {
            var whitelistedPlugins = new HashSet<string>
            {
                "com.spt-aki.core",
                "com.spt-aki.custom",
                "com.spt-aki.debugging",
                "com.spt-aki.singleplayer",
                "com.bepis.bepinex.configurationmanager",
                "com.terkoiz.freecam",
                "com.sinai.unityexplorer",
                "com.cwx.debuggingtool-dxyz",
                "com.cwx.debuggingtool",
                "xyz.drakia.botdebug",
                "com.kobrakon.camunsnap",
                "RuntimeUnityEditor"
            };

            var disallowedPlugins = Chainloader.PluginInfos.Values.Select(pi => pi.Metadata.GUID).Except(whitelistedPlugins).ToArray();
            if (disallowedPlugins.Any())
            {
                logger.LogError($"One or more non-whitelisted plugins were detected. Mods are not allowed in BleedingEdge builds of SPT. Illegal plugins:\n{string.Join("\n", disallowedPlugins)}");
                throw new Exception("Non-debug client mods have been detected. Mods are not allowed in BleedingEdge builds of SPT - please remove them before playing!");
            }
        }
    }
}