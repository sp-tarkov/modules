using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Patching;
using SPT.Custom.Models;
using EFT.UI;
using EFT.UI.Matchmaker;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Utils;

namespace SPT.Custom.Patches
{
    public class SetPreRaidSettingsScreenDefaultsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(MatchmakerOfflineRaidScreen))
                .SingleCustom(m => m.Name == nameof(MatchmakerOfflineRaidScreen.Show) && m.GetParameters().Length == 1);
        }

        [PatchPrefix]
        public static void PatchPrefix(object controller, UpdatableToggle ____offlineModeToggle)
        {
            //var raidSettings = Traverse.Create(controller).Field<RaidSettings>("RaidSettings").Value;
            var offlineRaidSettings = Traverse.Create(controller).Field<RaidSettings>("OfflineRaidSettings").Value;

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
        public static void PatchPostfix(MatchmakerOfflineRaidScreen __instance, DefaultUIButton ____changeSettingsButton, UiElementBlocker ____onlineBlocker)
        {
            ____onlineBlocker.gameObject.SetActive(false);
            ____changeSettingsButton.Interactable = true;
            __instance.transform.Find("Content/WarningPanelHorLayout").gameObject.SetActive(false);
        }
    }
}
