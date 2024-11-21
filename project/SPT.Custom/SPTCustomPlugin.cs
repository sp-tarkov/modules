using System;
using SPT.Common;
using SPT.Custom.Patches;
using SPT.Custom.Utils;
using SPT.Reflection.Utils;
using BepInEx;
using UnityEngine;
using BepInEx.Logging;

namespace SPT.Custom
{
    [BepInPlugin("com.SPT.custom", "SPT.Custom", SPTPluginInfo.PLUGIN_VERSION)]
    public class SPTCustomPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: SPT.Custom");

            try
            {
                // Bundle patches should always load first - DO NOT REMOVE
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();

                // Still need
                new DisableNonHalloweenExitsDuringEventPatch().Enable();
                new SendFleaListingTaxAmountToServerPatch().Enable();
                new AddTraitorScavsPatch().Enable();
                new CustomAiPatch().Enable();
                new SaveSettingsToSptFolderPatch().Enable();
                new SaveRegistryToSptFolderPatches().Enable();
                new QTEPatch().Enable();
                new RedirectClientImageRequestsPatch().Enable();
                new DisableGameModeAdjustButtonPatch().Enable();
                new FixPmcSpawnParamsNullErrorPatch().Enable();
                new SetPreRaidSettingsScreenDefaultsPatch().Enable();
                new CoreDifficultyPatch().Enable();
                new BotDifficultyPatch().Enable();
                new VersionLabelPatch().Enable();
                new FixScavWarNullErrorWithMarkOfUnknownPatch().Enable();
				new MergeScavPmcQuestsOnInventoryLoadPatch().Enable();
                new CopyPmcQuestsToPlayerScavPatch().Enable();
                new FixBossesHavingNoFollowersOnMediumAiAmount().Enable();
                new FixAirdropCrashPatch().Enable();
				//new AllowAirdropsInPvEPatch().Enable();
                new MemoryCollectionPatch().Enable();

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
