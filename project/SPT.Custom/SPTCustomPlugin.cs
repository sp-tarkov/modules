using System;
using SPT.Common;
using SPT.Custom.Patches;
using SPT.Custom.Utils;
using SPT.Reflection.Utils;
using SPT.SinglePlayer.Utils.MainMenu;
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
                new BotCalledDataTryCallPatch().Enable();
                new BotCallForHelpCallBotPatch().Enable();
                new BotOwnerDisposePatch().Enable();
                new CheckAndAddEnemyPatch().Enable();
                new AddEnemyToAllGroupsInBotZonePatch().Enable();
                new CustomAiPatch().Enable();
                new AddTraitorScavsPatch().Enable();
                new PmcFirstAidPatch().Enable();
                new SettingsLocationPatch().Enable();
                new SetLocationIdOnRaidStartPatch().Enable();
                new RagfairFeePatch().Enable();
                new ScavQuestPatch().Enable();
				new ScavItemCheckmarkPatch().Enable();
				new HalloweenExtractPatch().Enable();
                
                // Needed but needs editing
                new IsEnemyPatch().Enable();
                
                // Still need
                new QTEPatch().Enable();
                new FileCachePatch().Enable();
                new BotSelfEnemyPatch().Enable();
                new DisablePvEPatch().Enable();
                new ClampRagdollPatch().Enable();
                new PMCSpawnParamPatch().Enable();
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
