using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Aki.SinglePlayer.Patches.ScavMode
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
        private static readonly Dictionary<string, LocationSettingsClass.Location> originalLocationSettings = new Dictionary<string, LocationSettingsClass.Location>();

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "TarkovApplication");
            var desiredMethod = Array.Find(desiredType.GetMethods(PatchConstants.PrivateFlags), IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private bool IsTargetMethod(MethodInfo arg)
        {
            // method_43 as of 26535
            var parameters = arg.GetParameters();
            return parameters.Length == 2
                && parameters[0]?.Name == "timeAndWeather"
                && parameters[1]?.Name == "timeHasComeScreenController"
                && arg.ReturnType == typeof(Task);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref RaidSettings ____raidSettings)
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

            // Set new raid time
            ____raidSettings.SelectedLocation.EscapeTimeLimit = serverResult.RaidTimeMinutes;

            // Reset survival time
            AdjustSurviveTimeForExtraction(serverResult.OriginalSurvivalTimeSeconds);
            if (serverResult.NewSurviveTimeSeconds.HasValue)
            {
                AdjustSurviveTimeForExtraction(serverResult.NewSurviveTimeSeconds.Value);
            }
            ResetMapExits(____raidSettings.SelectedLocation, originalLocationSettings[currentMapId]);
            if (serverResult.ExitChanges != null && serverResult.ExitChanges.Count > 0)
            {
                AdjustExtracts(____raidSettings.SelectedLocation, serverResult.ExitChanges);
            }

            ConsoleScreen.LogError($"Finished");
            return true; // Do original method
        }

        private static void AdjustExtracts(LocationSettingsClass.Location location, List<ExitChanges> exitChangesToApply)
        {
            // Loop over each exit change from server
            foreach (var exitChange in exitChangesToApply)
            {
                // Find the client exit we want to make changes to
                var exitToChange = location.exits.First(x => x.Name == exitChange.Name);
                if (exitToChange == null)
                {
                    ConsoleScreen.LogError($"Exit with Id: {exitChange.Name} not found, skipping");

                    continue;
                }

                if (exitChange.Chance.HasValue)
                {
                    ConsoleScreen.LogError($"Changed exit ${exitChange.Name} chance from {exitToChange.Chance} to {exitChange.Chance.Value}");
                    exitToChange.Chance = exitChange.Chance.Value;
                }

                if (exitChange.MinTime.HasValue)
                {
                    ConsoleScreen.LogError($"Changed exit ${exitChange.Name} MinTime from {exitToChange.MinTime} to {exitChange.MinTime.Value}");
                    ConsoleScreen.LogError($"Changed exit ${exitChange.Name} MaxTime from {exitToChange.MaxTime} to {exitChange.MaxTime.Value}");
                    exitToChange.MinTime = exitChange.MinTime.Value;
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
                    ConsoleScreen.LogError($"Unable to find exit with name: {cachedExit.Name}, skipping");

                    continue;
                }

                // Reset values to those from cache
                if (clientLocationExit.Chance != cachedExit.Chance)
                {
                    ConsoleScreen.LogError($"Reset exit ${cachedExit.Name} Chance from {cachedExit.Chance} to {clientLocationExit.Chance}");
                    clientLocationExit.Chance = cachedExit.Chance;
                }
                if (clientLocationExit.MinTime != cachedExit.MinTime)
                {
                    ConsoleScreen.LogError($"Reset exit ${cachedExit.Name} MinTime from {cachedExit.MinTime} to {clientLocationExit.MinTime}");
                    clientLocationExit.MinTime = cachedExit.MinTime;
                }

                if (clientLocationExit.MaxTime != cachedExit.MaxTime)
                {
                    ConsoleScreen.LogError($"Reset exit ${cachedExit.Name} MaxTime from {cachedExit.MaxTime} to {clientLocationExit.MaxTime}");
                    clientLocationExit.MaxTime = cachedExit.MaxTime;
                }
            }
        }

        private static void AdjustSurviveTimeForExtraction(int newSurvivalTimeSeconds)
        {
            var matchEndConfig = Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd;
            ConsoleScreen.LogError($"Changed survive time to {newSurvivalTimeSeconds}");
            matchEndConfig.SurvivedTimeRequirement = newSurvivalTimeSeconds;
        }
    }
}