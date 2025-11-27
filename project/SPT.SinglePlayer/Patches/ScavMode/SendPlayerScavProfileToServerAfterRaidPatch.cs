using System.Linq;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.ScavMode;

/// <summary>
/// Get Profile at LocalGame End to use in SendPlayerScavProfileToServerAfterRaidPatch
/// </summary>
public class GetProfileAtEndOfRaidPatch : ModulePatch
{
    public static ProfileDescriptor ProfileDescriptor { get; private set; }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop));
    }

    [PatchPrefix]
    public static void PatchPrefix(LocalGame __instance, ExitStatus exitStatus)
    {
        if (exitStatus == ExitStatus.Runner)
        {
            __instance.Profile_0.SetSpawnedInSession(false);
        }
        ProfileDescriptor = new ProfileDescriptor(
            __instance.Profile_0,
            VisualsOnlySearchController.Instance /* Has 2 methods */
        );
    }
}

/// <summary>
/// Get profile from other patch (GetProfileAtEndOfRaidPatch)
/// If our profile is savage Create new Session.AllProfiles and pass in our own profile to allow us to use the ScavengerInventoryScreen
/// </summary>
public class SendPlayerScavProfileToServerAfterRaidPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SessionResultShowOperation), nameof(SessionResultShowOperation.Init));
    }

    [PatchPrefix]
    public static void PatchPrefix(SessionResultShowOperation __instance)
    {
        Profile profile = new(GetProfileAtEndOfRaidPatch.ProfileDescriptor);

        // Player is PMC, skip patch
        if (profile.Side != EPlayerSide.Savage)
        {
            return;
        }

        // Only do below when player is a scav
        var session = (ClientBackendSession) __instance._session;
        session.AllProfiles = [session.AllProfiles.First(x => x.Side != EPlayerSide.Savage), profile];
        session.ProfileOfPet.LearnAll();

        // Send scav profile to server so it knows of the items we might transfer
        RequestHandler.PutJson(
            "/raid/profile/scavsave",
            GetProfileAtEndOfRaidPatch.ProfileDescriptor.ToUnparsedData([]).JObject.ToString()
        );
    }
}
