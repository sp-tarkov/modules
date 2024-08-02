using System;
using SPT.Common;
using SPT.SinglePlayer.Patches.MainMenu;
using SPT.SinglePlayer.Patches.Progression;
using SPT.SinglePlayer.Patches.Quests;
using SPT.SinglePlayer.Patches.RaidFix;
using SPT.SinglePlayer.Patches.ScavMode;
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
                new TinnitusFixPatch().Enable();
                new EmptyInfilFixPatch().Enable();
                new MaxBotPatch().Enable();
                new PostRaidHealingPricePatch().Enable();
                new HideoutQuestIgnorePatch().Enable();
                new SpawnProcessNegativeValuePatch().Enable();
                new SpawnPmcPatch().Enable();
                // new ScavRepAdjustmentPatch().Enable();
                // new ScavSellAllPriceStorePatch().Enable();
                // new ScavSellAllRequestPatch().Enable();


                // Still need
                // new SmokeGrenadeFuseSoundFixPatch().Enable(); TODO: refactor as it causes exceptions to be thrown when grenade is tossed by player
                new ScavExperienceGainPatch().Enable();
                new DisablePMCExtractsForScavsPatch().Enable();
                new ScavExfilPatch().Enable();
                new ScavProfileLoadPatch().Enable();
                new ScavPrefabLoadPatch().Enable();
                new DisableReadyLocationReadyPatch().Enable();
                new BotTemplateLimitPatch().Enable();
                new LoadOfflineRaidScreenPatch().Enable();
                new AmmoUsedCounterPatch().Enable();
                new ArmorDamageCounterPatch().Enable();
                new PluginErrorNotifierPatch().Enable();
                new GetNewBotTemplatesPatch().Enable();
                new LabsKeycardRemovalPatch().Enable();
                new MapReadyButtonPatch().Enable();
                new RemoveUsedBotProfilePatch().Enable();
                new ScavLateStartPatch().Enable();
                
                // 3.10.0
                new PVEModeWelcomeMessagePatch().Enable();
                new DisableMatchmakerPlayerPreviewButtonsPatch().Enable();
                new EnableRefForPVEPatch().Enable();
                new EnableRefIntermScreenPatch().Enable();
                new EnablePlayerScavPatch().Enable();
                new ScavFoundInRaidPatch().Enable();
                new GetProfileAtEndOfRaidPatch().Enable();
                new FixSavageInventoryScreenPatch().Enable();
                new InsuranceScreenPatch().Enable();
                new FixQuestAchieveControllersPatch().Enable();
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
