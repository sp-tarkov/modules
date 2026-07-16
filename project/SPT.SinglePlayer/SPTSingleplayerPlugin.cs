using System;
using BepInEx;
using SPT.Common;
using SPT.SinglePlayer.Patches.MainMenu;
using SPT.SinglePlayer.Patches.RaidFix;
using SPT.SinglePlayer.Patches.ScavMode;
using SPT.SinglePlayer.Utils.MainMenu;

namespace SPT.SinglePlayer;

[BepInPlugin("com.SPT.singleplayer", "SPT.Singleplayer", SPTPluginInfo.PLUGIN_VERSION)]
public class SPTSingleplayerPlugin : BaseUnityPlugin
{
    public void Awake()
    {
        Logger.LogInfo("Loading: SPT.SinglePlayer");

        try
        {
            // TODO: check if these patches are needed
            new TinnitusFixPatch().Enable(); // Probably needed
            new EmptyInfilFixPatch().Enable();
            new OverrideMaxAiAliveInRaidValuePatch().Enable();

            // Still need
            new DisablePMCExtractsForScavsPatch().Enable();
            new ScavExfilPatch().Enable();
            new ScavProfileLoadPatch().Enable();
            new ScavPrefabLoadPatch().Enable();
            new LoadOfflineRaidScreenPatch().Enable();
            new PluginErrorNotifierPatch().Enable();
            new RemoveUsedBotProfilePatch().Enable();
            new ScavLateStartPatch().Enable();
            new ScavSellAllPriceStorePatch().Enable();
            new ScavSellAllRequestPatch().Enable();
            new ScavRepAdjustmentPatch().Enable();

            // 3.10.0
            new DisableWelcomeToPVEModeMessagePatch().Enable();
            new DisableMatchmakerPlayerPreviewButtonsPatch().Enable();
            new GetProfileAtEndOfRaidPatch().Enable();
            new SendPlayerScavProfileToServerAfterRaidPatch().Enable();
            new RemoveStashUpgradeLabelPatch().Enable();
            new RemoveClothingItemExternalObtainLabelPatch().Enable();
            new ForceRaidModeToLocalPatch().Enable();
            new ScavIsPlayerEnemyPatch().Enable();
            new FixKeyAlreadyExistsErrorOnAchievementPatch().Enable();

            // 3.11.0
            new ScavPrestigeFixPatch().Enable();
            new DisableUseBSGServersCheckbox().Enable();
            new PmcBotSidePatch().Enable();
            new QuestAchievementRewardInRaidPatch().Enable();

            // 4.0.0
            ReadyButtonPatches.Patch();
            new DisableBuffLoggingPatch().Enable();
            new RemoveStashUpgradeLabelPatch2().Enable();

            new RemoveTransitionRaidModeSetPatch().Enable();
            new FixDisableBossSpawningOptionPatch().Enable();
            new DisableHideoutCounterResetPatch().Enable();
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
        TraderCustomizationManager.AddModdedTraders();
    }
}
