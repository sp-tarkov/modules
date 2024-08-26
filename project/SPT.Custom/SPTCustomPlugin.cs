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
                
                // TODO: check if these patches are needed
				new DisableNonHalloweenExitsDuringEventPatch().Enable();
                // new AllScavsHostileHostileToPlayerScavPatch().Enable();
                //new IsEnemyPatch().Enable(); // TODO: can probably remove, this is handled by server data sent to client on raid start
                
                // Still need
                new SendFleaListingTaxAmountToServerPatch().Enable();
                new AddTraitorScavsPatch().Enable();
                new CustomAiPatch().Enable();
                new SaveSettingsToSptFolderPatch().Enable();
                new QTEPatch().Enable();
                new RedirectClientImageRequestsPatch().Enable();
                new DisableGameModeAdjustButtonPatch().Enable();
                new FixPmcSpawnParamsNullErrorPatch().Enable();
                new SetPreRaidSettingsScreenDefaultsPatch().Enable();
                new CoreDifficultyPatch().Enable();
                new BotDifficultyPatch().Enable();
                //new BossSpawnChancePatch().Enable(); // Tested factory, Tagilla does not spawn 100% of the time with patch disabled
                new VersionLabelPatch().Enable();
                new FixScavWarNullErrorWithMarkOfUnknownPatch().Enable();
				new MergeScavPmcQuestsOnInventoryLoadPatch().Enable();
                new CopyPmcQuestsToPlayerScavPatch().Enable();

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
