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
                new TinnitusFixPatch().Enable();
                new SmokeGrenadeFuseSoundFixPatch().Enable();
                new EmptyInfilFixPatch().Enable();
                new ScavExperienceGainPatch().Enable();
                new MainMenuControllerPatch().Enable();
                new HealthControllerPatch().Enable();
                new PlayerPatch().Enable();
                new MaxBotPatch().Enable();
                new PostRaidHealingPricePatch().Enable();
                new InRaidQuestAvailablePatch().Enable();
                new ExfilPointManagerPatch().Enable();
                new ScavEncyclopediaPatch().Enable();
                new HideoutQuestIgnorePatch().Enable();
                new SpawnProcessNegativeValuePatch().Enable();
                new SpawnPmcPatch().Enable();
                new ScavProfileLoadPatch().Enable();
                new ScavPrefabLoadPatch().Enable();
                new ScavExfilPatch().Enable();
                new LighthouseBridgePatch().Enable();
                new LighthouseTransmitterPatch().Enable();
                new InsuredItemManagerStartPatch().Enable();
                new GetTraderServicesPatch().Enable();
                new PurchaseTraderServicePatch().Enable();
                new LightKeeperServicesPatch().Enable();
                
                // Still need
                new DisableReadyLocationReadyPatch().Enable();
                new BotTemplateLimitPatch().Enable();
                new LoadOfflineRaidScreenPatch().Enable();
                new AmmoUsedCounterPatch().Enable();
                new ArmorDamageCounterPatch().Enable();
                new ScavRepAdjustmentPatch().Enable();
                new PluginErrorNotifierPatch().Enable();
                new GetNewBotTemplatesPatch().Enable();
                new ScavLateStartPatch().Enable();
                new LabsKeycardRemovalPatch().Enable();
                new MapReadyButtonPatch().Enable();
                new RemoveUsedBotProfilePatch().Enable();
                new ScavSellAllPriceStorePatch().Enable();
                new ScavSellAllRequestPatch().Enable();
                
                // 3.10.0
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
