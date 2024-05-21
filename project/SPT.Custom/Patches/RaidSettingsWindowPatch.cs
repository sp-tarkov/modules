using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Patching;
using SPT.Custom.Models;
using EFT.UI;
using EFT.UI.Matchmaker;
using System.Reflection;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// The purpose of this patch is to make RaidSettingsWindow not reset the values to BSG default
    /// Keeping our own from InRaid.json therefore not defaulting to bosses being disabled.
    /// </summary>
    public class RaidSettingsWindowPatch : ModulePatch
    {
        /// <summary>
        /// Target method should have ~20 .UpdateValue() calls in it
        /// </summary>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RaidSettingsWindow), nameof(RaidSettingsWindow.method_8));
        }

        [PatchPrefix]
        private static bool PatchPreFix(
            UpdatableToggle ____enableBosses,
            UpdatableToggle ____scavWars,
            UpdatableToggle ____taggedAndCursed,
            DropDownBox ____aiDifficultyDropdown,
            DropDownBox ____aiAmountDropdown,
            UpdatableToggle ____randomWeatherToggle,
            UpdatableToggle ____randomTimeToggle)
        {
            var json = RequestHandler.GetJson("/singleplayer/settings/raid/menu");
            var settings = Json.Deserialize<DefaultRaidSettings>(json);

            ____enableBosses.UpdateValue(settings.BossEnabled);
            ____scavWars.UpdateValue(false);
            ____taggedAndCursed.UpdateValue(settings.TaggedAndCursed);
            ____aiDifficultyDropdown.UpdateValue((int)settings.AiDifficulty);
            ____aiAmountDropdown.UpdateValue((int)settings.AiAmount);

            ____randomWeatherToggle.UpdateValue(settings.RandomWeather);
            ____randomTimeToggle.UpdateValue(settings.RandomTime);

            return false;
        }
    }
}