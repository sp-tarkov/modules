using System;
using Aki.Common;
using Aki.Core.Patches;
using BepInEx;

namespace Aki.Core
{
    [BepInPlugin("com.spt-aki.core", "AKI.Core", AkiPluginInfo.PLUGIN_VERSION)]
	class AkiCorePlugin : BaseUnityPlugin
	{
        public void Awake()
        {
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
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.Core");
        }
    }
}
