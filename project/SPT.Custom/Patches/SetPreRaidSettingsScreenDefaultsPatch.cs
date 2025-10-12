using System.Reflection;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using static EFT.UI.Matchmaker.MatchmakerOfflineRaidScreen;

namespace SPT.Custom.Patches;

public class SetPreRaidSettingsScreenDefaultsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools
            .GetDeclaredMethods(typeof(MatchmakerOfflineRaidScreen))
            .SingleCustom(m => m.Name == nameof(MatchmakerOfflineRaidScreen.Show) && m.GetParameters().Length == 1);
    }

    [PatchPrefix]
    public static void PatchPrefix(CreateRaidSettingsForProfileClass controller, UpdatableToggle ____offlineModeToggle)
    {
        var offlineRaidSettings = controller.OfflineRaidSettings;

        // Default checkbox to be unchecked so we're in PvE
        ____offlineModeToggle.isOn = false;

        // Get settings from server
        var json = RequestHandler.GetJson("/singleplayer/settings/raid/menu");
        var defaultSettings = Json.Deserialize<DefaultRaidSettings>(json);

        // TODO: Not all settings are used and they also don't cover all the new settings that are available client-side
        if (defaultSettings == null)
        {
            return;
        }

        // We use PVE mode from Tarkov now we need to modify PVE MODE instead of ONLINE Mode

        offlineRaidSettings.BotSettings.BotAmount = defaultSettings.AiAmount;
        offlineRaidSettings.WavesSettings.BotAmount = defaultSettings.AiAmount;
        offlineRaidSettings.WavesSettings.BotDifficulty = defaultSettings.AiDifficulty;
        offlineRaidSettings.WavesSettings.IsBosses = defaultSettings.BossEnabled;
        offlineRaidSettings.BotSettings.IsScavWars = false;
        offlineRaidSettings.WavesSettings.IsTaggedAndCursed = defaultSettings.TaggedAndCursed;
        offlineRaidSettings.TimeAndWeatherSettings.IsRandomWeather = defaultSettings.RandomWeather;
        offlineRaidSettings.TimeAndWeatherSettings.IsRandomTime = defaultSettings.RandomTime;
    }

    [PatchPostfix]
    public static void PatchPostfix(
        MatchmakerOfflineRaidScreen __instance,
        DefaultUIButton ____changeSettingsButton,
        UiElementBlocker ____onlineBlocker
    )
    {
        ____onlineBlocker.gameObject.SetActive(false);
        ____changeSettingsButton.Interactable = true;
        __instance.transform.Find("Content/WarningPanelHorLayout").gameObject.SetActive(false);
    }
}
