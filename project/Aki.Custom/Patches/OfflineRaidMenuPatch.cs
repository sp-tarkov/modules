using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Aki.Custom.Models;
using EFT.UI;
using EFT.UI.Matchmaker;
using System.Reflection;
using EFT;
using HarmonyLib;
using Aki.Reflection.Utils;

namespace Aki.Custom.Patches
{
    public class OfflineRaidMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(MatchmakerOfflineRaidScreen))
                .SingleCustom(m => m.Name == nameof(MatchmakerOfflineRaidScreen.Show) && m.GetParameters().Length == 1);
        }

        [PatchPrefix]
        private static void PatchPrefix(object controller, UpdatableToggle ____offlineModeToggle)
        {
            var raidSettings = Traverse.Create(controller).Field<RaidSettings>("RaidSettings").Value;

            raidSettings.RaidMode = ERaidMode.Local;

            // Default checkbox to be ticked
            ____offlineModeToggle.isOn = true;

            // get settings from server
            var json = RequestHandler.GetJson("/singleplayer/settings/raid/menu");
            var settings = Json.Deserialize<DefaultRaidSettings>(json);

            // TODO: Not all settings are used and they also don't cover all the new settings that are available client-side
            if (settings == null)
            {
                return;
            }

            raidSettings.BotSettings.BotAmount = settings.AiAmount;
            raidSettings.WavesSettings.BotAmount = settings.AiAmount;
            raidSettings.WavesSettings.BotDifficulty = settings.AiDifficulty;
            raidSettings.WavesSettings.IsBosses = settings.BossEnabled;
            raidSettings.BotSettings.IsScavWars = false;
            raidSettings.WavesSettings.IsTaggedAndCursed = settings.TaggedAndCursed;
            raidSettings.TimeAndWeatherSettings.IsRandomWeather = settings.RandomWeather;
            raidSettings.TimeAndWeatherSettings.IsRandomTime = settings.RandomTime;
        }

        [PatchPostfix]
        private static void PatchPostfix(MatchmakerOfflineRaidScreen __instance, UiElementBlocker ____onlineBlocker)
        {
            // Hide "no progression save" panel
            var warningPanel = __instance.transform.Find("Content/WarningPanelHorLayout").gameObject;
            warningPanel.SetActive(false);
            var spacer = __instance.transform.Find("Content/Space (1)").gameObject;
            spacer.SetActive(false);

            // Disable "Enable practice mode for this raid" toggle
            ____onlineBlocker.SetBlock(true, "Raids in SPT are always Offline raids. Don't worry - your progress will be saved!");
        }
    }
}
