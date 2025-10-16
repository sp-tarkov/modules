using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

/// <summary>
/// Credit to https://github.com/November75-SPT for finding and creating these patches
/// Fixes BotsEventsController being run without valid 'botSpawner' object, causing it to fail and prevent halloween events from occurring
/// </summary>
public class BotsControllerInitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsController),
            nameof(BotsController.Init));
    }

    [PatchPostfix]
    public static void PatchPostfix(BotsController __instance, LocationSettingsClass.Location.EventsDataClass events)
    {
        // Run it again with a non-null _botSpawner.
        __instance.EventsController = new BotsEventsController(
            __instance.BotGame.GameDateTime,
            __instance.Bots,
            __instance.ZonesLeaveController,
            __instance.BotSpawner,
            __instance.CoversData.AIPlaceInfoHolder,
            events,
            __instance.CoversData
        );

        __instance.EventsController.Activate();
    }
}

/// <summary>
/// Prevent event controller from running when `botSpawner` is null, so it can be run later after it has been hydrated
/// </summary>
public class BotsEventsControllerActivatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsEventsController),
            nameof(BotsEventsController.Activate));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotsEventsController __instance)
    {
        if (__instance.BotHalloweenEvent.Spawner == null)
        {
            Logger.LogDebug("__instance.BotHalloweenEvent.Spawner is null skip Activate");
            return false;
        }

        Logger.LogDebug("__instance.BotHalloweenEvent.Spawner is not null run Activate");
        return true;
    }
}
