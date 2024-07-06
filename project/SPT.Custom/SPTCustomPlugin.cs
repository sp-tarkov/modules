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
                // Bundle patches should always load first
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();
                new BossSpawnChancePatch().Enable();
                new BotDifficultyPatch().Enable();
                new CoreDifficultyPatch().Enable();
                new OfflineRaidMenuPatch().Enable();
                new VersionLabelPatch().Enable();
                new IsEnemyPatch().Enable();
                new BotCalledDataTryCallPatch().Enable();
                new BotCallForHelpCallBotPatch().Enable();
                new BotOwnerDisposePatch().Enable();
                new LocationLootCacheBustingPatch().Enable();
                new CheckAndAddEnemyPatch().Enable();
                new BotSelfEnemyPatch().Enable(); // needed
                new AddEnemyToAllGroupsInBotZonePatch().Enable();
                new CustomAiPatch().Enable();
                new AddTraitorScavsPatch().Enable();
                new ExitWhileLootingPatch().Enable();
                new QTEPatch().Enable();
                new PmcFirstAidPatch().Enable();
                new SettingsLocationPatch().Enable();
                new SetLocationIdOnRaidStartPatch().Enable();
                new RagfairFeePatch().Enable();
                new ScavQuestPatch().Enable();
                new FixBrokenSpawnOnSandboxPatch().Enable();
				new ScavItemCheckmarkPatch().Enable();
                new ResetTraderServicesPatch().Enable();
				new CultistAmuletRemovalPatch().Enable();
				new HalloweenExtractPatch().Enable();
                new ClampRagdollPatch().Enable();
                new DisablePvEPatch().Enable();
                new InsurancePlaceItem().Enable();
                new FileCachePatch().Enable();
                new PMCSpawnParamPatch().Enable();

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
