using System;
using SPT.Common;
using SPT.SinglePlayer.Patches.Healing;
using SPT.SinglePlayer.Patches.MainMenu;
using SPT.SinglePlayer.Patches.Progression;
using SPT.SinglePlayer.Patches.Quests;
using SPT.SinglePlayer.Patches.RaidFix;
using SPT.SinglePlayer.Patches.ScavMode;
using SPT.SinglePlayer.Patches.TraderServices;
using BepInEx;

namespace SPT.SinglePlayer
{
    [BepInPlugin("com.SPT.singleplayer", "spt.Singleplayer", SPTPluginInfo.PLUGIN_VERSION)]
    class SPTSingleplayerPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: SPT.SinglePlayer");

            try
            {
                // TODO: check if these patches are needed
                new MidRaidQuestChangePatch().Enable(); 
				new MidRaidAchievementChangePatch().Enable();
                new ScavSellAllPriceStorePatch().Enable();
                new ScavSellAllRequestPatch().Enable();
                
                new ScavExperienceGainPatch().Enable();
                new MainMenuControllerPatch().Enable();
                new PlayerPatch().Enable();
                new DisableReadyLocationReadyPatch().Enable();
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
                new InRaidQuestAvailablePatch().Enable();
                new VoIPTogglerPatch().Enable();
                new HealthControllerPatch().Enable();
                new LighthouseBridgePatch().Enable();
                new LighthouseTransmitterPatch().Enable();
                new EmptyInfilFixPatch().Enable();
                new SmokeGrenadeFuseSoundFixPatch().Enable();
                new PlayerToggleSoundFixPatch().Enable();
                new PluginErrorNotifierPatch().Enable();
                new SpawnProcessNegativeValuePatch().Enable();
                new InsuredItemManagerStartPatch().Enable();
                new MapReadyButtonPatch().Enable();
                new LabsKeycardRemovalPatch().Enable();
                new ScavLateStartPatch().Enable();
                new GetTraderServicesPatch().Enable();
                new PurchaseTraderServicePatch().Enable();
                new HideoutQuestIgnorePatch().Enable();
                new LightKeeperServicesPatch().Enable();
                new ScavEncyclopediaPatch().Enable();
                new ScavRepAdjustmentPatch().Enable();
                new AmmoUsedCounterPatch().Enable();
                new ArmorDamageCounterPatch().Enable();
                new PVEModeWelcomeMessagePatch().Enable();
                new DisableMatchmakerPlayerPreviewButtonsPatch().Enable();
                new EnableRefForPVEPatch().Enable();
                new EnableRefIntermScreenPatch().Enable();
                new EnablePlayerScavPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: SPT.SinglePlayer");
        }
    }
}
