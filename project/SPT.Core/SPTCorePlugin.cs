using System;
using BepInEx;
using SPT.Common;
using SPT.Core.Patches;

namespace SPT.Core
{
    [BepInPlugin("com.SPT.core", "SPT.Core", SPTPluginInfo.PLUGIN_VERSION)]
    public class SPTCorePlugin : BaseUnityPlugin
    {
        // Temp static logger field, remove along with plugin whitelisting before release
        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;

            Logger.LogInfo("Loading: SPT.Core");

            try
            {
                new ConsistencySinglePatch().Enable();
                new ConsistencyMultiPatch().Enable();
                new GameValidationPatch().Enable();
                new BattlEyePatch().Enable();
                new SslCertificatePatch().Enable();
                new UnityWebRequestPatch().Enable();
                new WebSocketSslValidationPatch().Enable();
                new Patch4001().Enable();
                new Patch4002().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");

                throw;
            }

            Logger.LogInfo("Completed: SPT.Core");
        }
    }
}
