using System;
using SPT.Common;
using SPT.Custom.Airdrops.Patches;
using SPT.Custom.BTR.Patches;
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
                // Fixed in live, no need for patch
                //new RaidSettingsWindowPatch().Enable();
                new OfflineRaidSettingsMenuPatch().Enable();
                // new SessionIdPatch().Enable();
                new VersionLabelPatch().Enable();
                new IsEnemyPatch().Enable();
                new BotCalledDataTryCallPatch().Enable();
                new BotCallForHelpCallBotPatch().Enable();
                new BotOwnerDisposePatch().Enable();
                new LocationLootCacheBustingPatch().Enable();
                //new AddSelfAsEnemyPatch().Enable();
                new CheckAndAddEnemyPatch().Enable();
                new BotSelfEnemyPatch().Enable(); // needed
                new AddEnemyToAllGroupsInBotZonePatch().Enable();
                new AirdropPatch().Enable();
                new AirdropFlarePatch().Enable();
                //new AddSptBotSettingsPatch().Enable();
                new CustomAiPatch().Enable();
                new AddTraitorScavsPatch().Enable();
                new ExitWhileLootingPatch().Enable();
                new QTEPatch().Enable();
                new PmcFirstAidPatch().Enable();
                new SettingsLocationPatch().Enable();
                new SetLocationIdOnRaidStartPatch().Enable();
                //new RankPanelPatch().Enable();
                new RagfairFeePatch().Enable();
                new ScavQuestPatch().Enable();
                new FixBrokenSpawnOnSandboxPatch().Enable();
                new BTRControllerInitPatch().Enable();
                new BTRPathLoadPatch().Enable();
                new BTRActivateTraderDialogPatch().Enable();
                new BTRInteractionPatch().Enable();
                new BTRExtractPassengersPatch().Enable();
                new BTRBotAttachPatch().Enable();
                new BTRReceiveDamageInfoPatch().Enable();
                new BTRTurretCanShootPatch().Enable();
                new BTRTurretDefaultAimingPositionPatch().Enable();
                new BTRIsDoorsClosedPath().Enable();
                new BTRPatch().Enable();
                new BTRTransferItemsPatch().Enable();
                new BTREndRaidItemDeliveryPatch().Enable();
                new BTRDestroyAtRaidEndPatch().Enable();
                new BTRVehicleMovementSpeedPatch().Enable();
                new BTRPathConfigMapPrefixPatch().Enable();
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
