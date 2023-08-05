using System;
using Aki.Common;
using Aki.SinglePlayer.Patches.Healing;
using Aki.SinglePlayer.Patches.MainMenu;
using Aki.SinglePlayer.Patches.Progression;
using Aki.SinglePlayer.Patches.Quests;
using Aki.SinglePlayer.Patches.RaidFix;
using Aki.SinglePlayer.Patches.ScavMode;
using BepInEx;

namespace Aki.SinglePlayer
{
    [BepInPlugin("com.spt-aki.singleplayer", "AKI.Singleplayer", AkiPluginInfo.PLUGIN_VERSION)]
    class AkiSingleplayerPlugin : BaseUnityPlugin
    {
        public AkiSingleplayerPlugin()
        {
            Logger.LogInfo("Loading: Aki.SinglePlayer");

            try
            {
                new OfflineSaveProfilePatch().Enable();
                new OfflineSpawnPointPatch().Enable();
                new ExperienceGainPatch().Enable();
                new ScavExperienceGainPatch().Enable();
                new MainMenuControllerPatch().Enable();
                new PlayerPatch().Enable();
                new SelectLocationScreenPatch().Enable();
                new InsuranceScreenPatch().Enable();
                new BotTemplateLimitPatch().Enable();
                new GetNewBotTemplatesPatch().Enable();
                new RemoveUsedBotProfilePatch().Enable();
                new DogtagPatch().Enable();
                new LoadOfflineRaidScreenPatch().Enable();
                new ScavPrefabLoadPatch().Enable();
                new ScavProfileLoadPatch().Enable();
                new ScavExfilPatch().Enable();
                new ExfilPointManagerPatch().Enable();
                new TinnitusFixPatch().Enable();
                new MaxBotPatch().Enable();
                new SpawnPmcPatch().Enable();
                new PostRaidHealingPricePatch().Enable();
                new EndByTimerPatch().Enable();
                new PostRaidHealScreenPatch().Enable();
                new VoIPTogglerPatch().Enable();
                new MidRaidQuestChangePatch().Enable();
                new HealthControllerPatch().Enable();
                new LighthouseBridgePatch().Enable();
                new LighthouseTransmitterPatch().Enable();
                new EmptyInfilFixPatch().Enable();
                new SmokeGrenadeFuseSoundFixPatch().Enable();
                new PlayerToggleSoundFixPatch().Enable();
                new PluginErrorNotifierPatch().Enable();
                new SpawnProcessNegativeValuePatch().Enable();
                new InsuredItemManagerStartPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.SinglePlayer");
        }
    }
}
