using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aki.Common;
using Aki.Core.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;

namespace Aki.Core
{
    [BepInPlugin("com.spt-aki.core", "AKI.Core", AkiPluginInfo.PLUGIN_VERSION)]
	class AkiCorePlugin : BaseUnityPlugin
	{
        // Temp static logger field, remove along with plugin whitelisting before release
        private static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;

            Logger.LogInfo("Loading: Aki.Core");

            try
            {
                new ConsistencySinglePatch().Enable();
                new ConsistencyMultiPatch().Enable();
                new BattlEyePatch().Enable();
                new SslCertificatePatch().Enable();
                new UnityWebRequestPatch().Enable();
                new WebSocketPatch().Enable();
                new TransportPrefixPatch().Enable();
                new PreventClientModsPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.Core");
        }

        /// <summary>
        /// See <see cref="PreventClientModsPatch"/> for explanation on why this is needed.
        /// Yes, I know this is jank but it's temporary and will be removed before release :)
        /// </summary>
        internal static void CheckForNonWhitelistedPlugins()
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
                _logger.LogError($"One or more non-whitelisted plugins were detected. Mods are not allowed in BleedingEdge builds of SPT. Illegal plugins:\n{string.Join("\n", disallowedPlugins)}");

                // Delay game shutdown by a little bit, since logging sometimes doesn't have enough time to write to file
                Task.Run(() =>
                {
                    Task.Delay(500);
                    Application.Quit(0);
                });
            }
        }
    }
}
