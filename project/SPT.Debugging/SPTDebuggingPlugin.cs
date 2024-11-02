using System;
using SPT.Common;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Debugging.Patches;
using BepInEx;
using SPT.Custom.Models;

namespace SPT.Debugging
{
    [BepInPlugin("com.SPT.debugging", "SPT.Debugging", SPTPluginInfo.PLUGIN_VERSION)]
    public class SPTDebuggingPlugin : BaseUnityPlugin
    {
        public static LoggingLevelResponse logLevel;

        public void Awake()
        {
            Logger.LogInfo("Loading: SPT.Debugging");

            try
            {
                new EndRaidDebug().Enable();
                new LoggerClassLogPatch().Enable();
                // new CoordinatesPatch().Enable();
                // new StaticLootDumper().Enable();
                // new ExfilDumper().Enable();

                // new PMCBotSpawnLocationPatch().Enable();
                new ReloadClientPatch().Enable();
                // new DumpyLibPatch().Enable();
			}
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: SPT.Debugging");
        }

        public void Start()
        {
            var loggingJson = RequestHandler.GetJson("/singleplayer/enableBSGlogging");
            logLevel = Json.Deserialize<LoggingLevelResponse>(loggingJson);
        }
    }
}
