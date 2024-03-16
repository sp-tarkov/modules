using System;
using Aki.Common;
using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Debugging.Patches;
using BepInEx;

namespace Aki.Debugging
{
    [BepInPlugin("com.spt-aki.debugging", "AKI.Debugging", AkiPluginInfo.PLUGIN_VERSION)]
    public class AkiDebuggingPlugin : BaseUnityPlugin
    {
        public static LoggingLevelResponse logLevel;

        public void Awake()
        {
            Logger.LogInfo("Loading: Aki.Debugging");

            try
            {
                new EndRaidDebug().Enable();
                new LoggerClassLogPatch().Enable();
                // new CoordinatesPatch().Enable();
                // new StaticLootDumper().Enable();

                // BTR debug command patches, can be disabled later
                //new BTRDebugCommandPatch().Enable();
                //new BTRDebugDataPatch().Enable();

                //new PMCBotSpawnLocationPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.Debugging");
        }

        public void Start()
        {
            var loggingJson = RequestHandler.GetJson("/singleplayer/enableBSGlogging");
            logLevel = Json.Deserialize<LoggingLevelResponse>(loggingJson);
        }
    }
}
