using System;
using Aki.Common;
using Aki.Debugging.BTR.Patches;
using Aki.Debugging.Patches;
using BepInEx;

namespace Aki.Debugging
{
    [BepInPlugin("com.spt-aki.debugging", "AKI.Debugging", AkiPluginInfo.PLUGIN_VERSION)]
    public class AkiDebuggingPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: Aki.Debugging");

            try
            {
                new EndRaidDebug().Enable();
                // new CoordinatesPatch().Enable();
                // new StaticLootDumper().Enable();
                new BTRInteractionPatch().Enable();
                new BTRBotAttachPatch().Enable();
                new BTRBotInitPatch().Enable();
				new BTRPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.Debugging");
        }
    }
}
