using System;
using Aki.Common;
using Aki.Custom.Airdrops.Patches;
using Aki.Custom.BTR.Patches;
using Aki.Custom.Patches;
using Aki.Custom.Utils;
using Aki.Reflection.Utils;
using Aki.SinglePlayer.Utils.MainMenu;
using BepInEx;
using UnityEngine;

namespace Aki.Custom
{
    [BepInPlugin("com.spt-aki.custom", "AKI.Custom", AkiPluginInfo.PLUGIN_VERSION)]
    class AkiCustomPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: Aki.Custom");

            try
            {
                // Bundle patches should always load first
                BundleManager.GetBundles();
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
                new AddSptBotSettingsPatch().Enable();
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
				new ScavItemCheckmarkPatch().Enable();
                new ResetTraderServicesPatch().Enable();
				new CultistAmuletRemovalPatch().Enable();

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

            Logger.LogInfo("Completed: Aki.Custom");
        }
    }
}
