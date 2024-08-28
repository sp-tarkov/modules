using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    /// <summary>
    /// Make alterations to the _raidSettings values prior to them being used to create a local game
    /// ____raidSettings.SelectedLocation.EscapeTimeLimit
    /// ____raidSettings.SelectedLocation.exits
    /// Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd.SurvivedTimeRequirement
    /// </summary>
    public class ScavLateStartPatch : ModulePatch
    {
        // A cache of Location settings before any edits were made
        private static readonly Dictionary<string, LocationSettingsClass.Location> originalLocationSettings = new();

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(TarkovApplication);
            var desiredMethod = Array.Find(desiredType.GetMethods(PatchConstants.PublicDeclaredFlags), IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private bool IsTargetMethod(MethodInfo arg)
        {
            // method_46 as of 32128
            var parameters = arg.GetParameters();
            return parameters.Length == 5 
                   && parameters[0]?.Name == "gameWorld"
                   && parameters[1]?.Name == "timeAndWeather"
                   && parameters[2]?.Name == "timeHasComeScreenController"
                   && parameters[3]?.Name == "metricsEvents"
                   && parameters[4]?.Name == "metricsConfig"
                   && arg.ReturnType == typeof(Task);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref RaidSettings ____raidSettings)
        {
            var currentMapId = ____raidSettings.SelectedLocation.Id;

            // Cache map info for use later 
            if (!originalLocationSettings.ContainsKey(currentMapId))
            {
                originalLocationSettings.Add(currentMapId, ____raidSettings.SelectedLocation);
            }

            // Create request and send to server, parse response
            var request = new RaidTimeRequest(____raidSettings.Side, currentMapId);
            var json = RequestHandler.PostJson("/singleplayer/settings/getRaidTime", Json.Serialize(request));
            var serverResult = Json.Deserialize<RaidTimeResponse>(json);

            // Capture the changes that will be made to the raid so they can be easily accessed by modders
            Utils.InRaid.RaidChangesUtil.UpdateRaidChanges(____raidSettings, serverResult);

            // Set new raid time
            ____raidSettings.SelectedLocation.EscapeTimeLimit = serverResult.RaidTimeMinutes;

            // Handle survival time changes
            AdjustSurviveTimeForExtraction(serverResult.NewSurviveTimeSeconds.HasValue
                ? serverResult.NewSurviveTimeSeconds.Value
                : serverResult.OriginalSurvivalTimeSeconds);

            // Handle exit changes
            ResetMapExits(____raidSettings.SelectedLocation, originalLocationSettings[currentMapId]);
            if (serverResult.ExitChanges != null && serverResult.ExitChanges.Count > 0)
            {
                AdjustMapExits(____raidSettings.SelectedLocation, serverResult.ExitChanges);
            }

            // Confirm that all raid changes are complete
            Utils.InRaid.RaidChangesUtil.ConfirmRaidChanges();

            return true; // Do original method
        }

        private static void AdjustMapExits(LocationSettingsClass.Location location, List<ExitChanges> exitChangesToApply)
        {
            // Loop over each exit change from server
            foreach (var exitChange in exitChangesToApply)
            {
                // Find the client exit we want to make changes to
                var exitToChange = location.exits.FirstOrDefault(x => x.Name == exitChange.Name);
                if (exitToChange == null)
                {
                    Logger.LogDebug($"Exit with Id: {exitChange.Name} not found, skipping");
                    continue;
                }

                if (exitChange.Chance.HasValue)
                {
                    exitToChange.Chance = exitChange.Chance.Value;
                }

                if (exitChange.MinTime.HasValue)
                {
                    exitToChange.MinTime = exitChange.MinTime.Value;
                }

                if (exitChange.MaxTime.HasValue)
                {
                    exitToChange.MaxTime = exitChange.MaxTime.Value;
                }
            }
        }

        private static void ResetMapExits(LocationSettingsClass.Location clientLocation, LocationSettingsClass.Location cachedLocation)
        {
            // Iterate over cached original map data
            foreach (var cachedExit in cachedLocation.exits)
            {
                // Find client exit
                var clientLocationExit = clientLocation.exits.FirstOrDefault(x => x.Name == cachedExit.Name);
                if (clientLocationExit == null)
                {
                    Logger.LogDebug($"Unable to find exit with name: {cachedExit.Name}, skipping");

                    continue;
                }

                // Reset values to those from cache
                clientLocationExit.Chance = cachedExit.Chance;
                clientLocationExit.MinTime = cachedExit.MinTime;
                clientLocationExit.MaxTime = cachedExit.MaxTime;
            }
        }

        private static void AdjustSurviveTimeForExtraction(int newSurvivalTimeSeconds)
        {
            var matchEndConfig = Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd;
            matchEndConfig.SurvivedTimeRequirement = newSurvivalTimeSeconds;
        }
    }
}