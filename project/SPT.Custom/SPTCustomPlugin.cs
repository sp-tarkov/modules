using System;
using BepInEx;
using SPT.Common;
using SPT.Custom.Patches;
using SPT.Custom.Utils;
using UnityEngine;

namespace SPT.Custom;

[BepInPlugin("com.SPT.custom", "SPT.Custom", SPTPluginInfo.PLUGIN_VERSION)]
public class SPTCustomPlugin : BaseUnityPlugin
{
    internal static GameObject HookObject;

    public void Awake()
    {
        Logger.LogInfo("Loading: SPT.Custom");
        HookObject = new GameObject();
        HookObject.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(HookObject);

        try
        {
            // Bundle patch should always load first - DO NOT REMOVE
            new EasyAssetsPatch().Enable();

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
            new CopyPmcQuestsAndWishlistToPlayerScavPatch().Enable();
            new FixBossesHavingNoFollowersOnMediumAiAmount().Enable();
            new FixAirdropCrashPatch().Enable();
            new FixAirdropFlareDisposePatch().Enable();
            new AllowAirdropsInPvEPatch().Enable();
            new MemoryCollectionPatch().Enable();

            // 3.11
            new EnablePrestigeTabPatch().Enable();
            new MatchStartServerLocationPatch().Enable();
            new SpawnPointAIPlayerBotLimitPatch().Enable();
            new SpawnPointNearestPlayerAIPatch().Enable();

            // Halloween event fixes
            new BotsControllerInitPatch().Enable();
            new BotsEventsControllerActivatePatch().Enable();

            HookObject.AddComponent<MenuNotificationManager>();
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED"
            );
            Logger.LogError($"{GetType().Name}: {ex}");
            MessageBoxHelper.Show(
                $"A patch in {GetType().Name} FAILED. {ex.Message}. SUBSEQUENT PATCHES HAVE NOT LOADED, CHECK LOG (/bepinex/LogOutput.log) FOR MORE DETAILS",
                "ERROR",
                MessageBoxHelper.MessageBoxType.OK
            );
            Application.Quit();

            throw;
        }

        Logger.LogInfo("Completed: SPT.Custom");
    }
}