using System;
using System.Reflection;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using SPT.SinglePlayer.Models.ScavMode;

namespace SPT.SinglePlayer.Patches.ScavMode;

/// <summary>
/// Make alterations to the survive time requirement prior to it being used to create a local game
/// Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd.SurvivedTimeRequirement
/// </summary>
public class ScavLateStartPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        var desiredType = typeof(TarkovApplication);
        var desiredMethod = Array.Find(desiredType.GetMethods(PatchConstants.PublicDeclaredFlags), IsTargetMethod);

        Logger.LogDebug($"{this.GetType().Name} Type: {desiredType.Name}");
        Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

        return desiredMethod;
    }

    private bool IsTargetMethod(MethodInfo arg)
    {
        // method_46 as of 32128
        var parameters = arg.GetParameters();
        return parameters.Length == 5
            && parameters[0].Name == "gameWorld"
            && parameters[1].Name == "timeAndWeather"
            && parameters[2].Name == "timeHasComeScreenController"
            && parameters[3].Name == "metricsEvents"
            && parameters[4].Name == "metricsConfig"
            && arg.ReturnType == typeof(Task);
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref RaidSettings ____raidSettings)
    {
        var currentMapId = ____raidSettings.SelectedLocation.Id;

        // Create request and send to server, parse response
        var request = new RaidTimeRequest(____raidSettings.Side, currentMapId);
        var json = RequestHandler.PostJson("/singleplayer/settings/getRaidTime", Json.Serialize(request));
        var serverResult = Json.Deserialize<RaidTimeResponse>(json);

        // Capture the changes that will be made to the raid so they can be easily accessed by modders
        Utils.InRaid.RaidChangesUtil.UpdateRaidChanges(____raidSettings, serverResult);

        // Handle survival time changes
        AdjustSurviveTimeForExtraction(
            serverResult.NewSurviveTimeSeconds.HasValue
                ? serverResult.NewSurviveTimeSeconds.Value
                : serverResult.OriginalSurvivalTimeSeconds
        );

        // Confirm that all raid changes are complete
        Utils.InRaid.RaidChangesUtil.ConfirmRaidChanges();

        return true; // Do original method
    }

    private static void AdjustSurviveTimeForExtraction(int newSurvivalTimeSeconds)
    {
        var matchEndConfig = Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd;
        matchEndConfig.SurvivedTimeRequirement = newSurvivalTimeSeconds;
    }
}
