using System;
using SPT.Common;
using SPT.Custom.Patches;
using SPT.Custom.Utils;
using SPT.Reflection.Utils;
using BepInEx;
using UnityEngine;

namespace SPT.Custom
{
    [BepInPlugin("com.SPT.custom", "SPT.Custom", SPTPluginInfo.PLUGIN_VERSION)]
    class SPTCustomPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: SPT.Custom");

            try
            {
                // Bundle patches should always load first - DO NOT REMOVE
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();
                
                // TODO: check if these patches are needed
                //new AddEnemyTryCallFailureFixPatch().Enable(); // NOT NEEDED bsg do it now
                new BotCallForHelpWrongTargetLocationPatch().Enable();
                //new BotOwnerDisposePatch().Enable(); // NOT NEEDED bsg do it now
                new BotsGroupLetBossesShootPmcsPatch().Enable();
                new CustomAiPatch().Enable();
                new AddTraitorScavsPatch().Enable();
                new PmcTakesAgesToHealLimbsPatch().Enable();
                new SendFleaListingTaxAmountToServerPatch().Enable();
				new DisableNonHalloweenExitsDuringEventPatch().Enable();
                // new SetLocationIdOnRaidStartPatch().Enable();
                // new AllScavsHostileHostileToPlayerScavPatch().Enable();
                // new CopyPmcQuestsToPlayerScavPatch().Enable();
				// new MergeScavPmcQuestsOnInventoryLoadPatch().Enable();
                
                // Needed but needs editing
                new IsEnemyPatch().Enable();
                
                // Still need
                new SaveSettingsToSptFolderPatch().Enable();
                new QTEPatch().Enable();
                new RedirectClientImageRequestsPatch().Enable();
                new BotSelfEnemyPatch().Enable();
                new DisableGameModeAdjustButtonPatch().Enable();
                new FixPmcSpawnParamsNullErrorPatch().Enable();
                new SetPreRaidSettingsScreenDefaultsPatch().Enable();
                new CoreDifficultyPatch().Enable();
                new BotDifficultyPatch().Enable();
                new BossSpawnChancePatch().Enable();
                new LocationLootCacheBustingPatch().Enable();
                new VersionLabelPatch().Enable();
                new FixBotgroupMarkofTheUnknown().Enable();

                HookObject.AddOrGetComponent<MenuNotificationManager>();
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                MessageBoxHelper.Show($"A patch in {GetType().Name} FAILED. {ex.Message}. SUBSEQUENT PATCHES HAVE NOT LOADED, CHECK LOG FOR MORE DETAILS", "ERROR", MessageBoxHelper.MessageBoxType.OK);
                Application.Quit();

                throw;
            }

            Logger.LogInfo("Completed: SPT.Custom");
        }
    }
}
