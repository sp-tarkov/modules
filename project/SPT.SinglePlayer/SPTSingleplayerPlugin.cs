using System;
using SPT.Common;
using SPT.SinglePlayer.Patches.MainMenu;
using SPT.SinglePlayer.Patches.Progression;
using SPT.SinglePlayer.Patches.RaidFix;
using SPT.SinglePlayer.Patches.ScavMode;
using BepInEx;
using SPT.SinglePlayer.Utils.MainMenu;

namespace SPT.SinglePlayer
{
    [BepInPlugin("com.SPT.singleplayer", "spt.Singleplayer", SPTPluginInfo.PLUGIN_VERSION)]
    public class SPTSingleplayerPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: SPT.SinglePlayer");

            try
            {
                // TODO: check if these patches are needed
                new TinnitusFixPatch().Enable(); // Probably needed
                //new EmptyInfilFixPatch().Enable();
                new OverrideMaxAiAliveInRaidValuePatch().Enable();
                //new PostRaidHealingPricePatch().Enable(); // Client handles this now
                //new HideoutQuestIgnorePatch().Enable(); // Was only needed because FixQuestAchieveControllersPatch was causing issues 
                //new SpawnProcessNegativeValuePatch().Enable(); // Client handles this edge case, revisit if bot count keeps going up
                //new SpawnPmcPatch().Enable(); // 2.5+ years old, PMC spawn system very different, likely not needed
                //new FixQuestAchieveControllersPatch().Enable(); // Likely not needed, if cheevos don't appear, revisit patch

                // Still need
                new FixPostScavRaidXpShowingZeroPatch().Enable();
                new DisablePMCExtractsForScavsPatch().Enable();
                new ScavExfilPatch().Enable();
                new ScavProfileLoadPatch().Enable();
                new ScavPrefabLoadPatch().Enable();
                new DisableReadyLocationReadyPatch().Enable();
                //new BotTemplateLimitPatch().Enable(); // Not necessary, controls how many 'respawns' the wave has, different to what the original patches intent was when written
                new LoadOfflineRaidScreenPatch().Enable();
                new AmmoUsedCounterPatch().Enable(); // Necessary for fixing bug #773
                new PluginErrorNotifierPatch().Enable();
                new GetNewBotTemplatesPatch().Enable();
                new MapReadyButtonPatch().Enable();
                new RemoveUsedBotProfilePatch().Enable();
                new ScavLateStartPatch().Enable();
                new ScavSellAllPriceStorePatch().Enable();
                new ScavSellAllRequestPatch().Enable();
                new ScavRepAdjustmentPatch().Enable();
                
                // 3.10.0
                new PVEModeWelcomeMessagePatch().Enable();
                new DisableMatchmakerPlayerPreviewButtonsPatch().Enable();
                new EnableRefForPVEPatch().Enable();
                new EnableRefIntermScreenPatch().Enable();
                new EnablePlayerScavPatch().Enable();
                new ScavFoundInRaidPatch().Enable();
                new GetProfileAtEndOfRaidPatch().Enable();
                new FixSavageInventoryScreenPatch().Enable();
                //new InsuranceScreenPatch().Enable();
                new RemoveStashUpgradeLabelPatch().Enable();
				new RemoveClothingItemExternalObtainLabelPatch().Enable();
				new FixLocalRaidPatch().Enable();
				new ScavIsPlayerEnemyPatch().Enable();
			}
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: SPT.SinglePlayer");
        }

        public void Start()
        {
            TraderServiceManager.GetModdedTraderData();
        }
    }
}
