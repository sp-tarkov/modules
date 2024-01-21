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
                new BTRPathLoadPatch().Enable();
                new BTRActivateTraderDialogPatch().Enable();
                new BTRInteractionPatch().Enable();
                new BTRExtractPassengersPatch().Enable();
                new BTRBotAttachPatch().Enable();
                new BTRBotInitPatch().Enable();
                new BTRReceiveDamageInfoPatch().Enable();
                new BTRTurretCanShootPatch().Enable();
                new BTRTurretDefaultAimingPositionPatch().Enable();
                new BTRIsDoorsClosedPath().Enable();
                new BTRPatch().Enable();
                new BTRTransferItemsPatch().Enable();
                new BTREndRaidItemDeliveryPatch().Enable();

                // Debug command patches, can be disabled later
                new BTRDebugCommandPatch().Enable();
                new BTRDebugDataPatch().Enable();

                //new PMCBotSpawnLocationPatch().Enable();
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
