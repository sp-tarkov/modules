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
    public static CompleteProfileDescriptorClass ProfileDescriptor { get; private set; }

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
        ProfileDescriptor = new CompleteProfileDescriptorClass(
            __instance.Profile_0,
            GClass2073.Instance /* Has 2 methods */
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
        return AccessTools.Method(typeof(PostRaidHealthScreenClass), nameof(PostRaidHealthScreenClass.method_2));
    }

    [PatchPrefix]
    public static void PatchPrefix(PostRaidHealthScreenClass __instance)
    {
        Profile profile = new(GetProfileAtEndOfRaidPatch.ProfileDescriptor);

        // Player is PMC, skip patch
        if (profile.Side != EPlayerSide.Savage)
        {
            return;
        }

        // Only do below when player is a scav
        var session = (ProfileEndpointFactoryAbstractClass)__instance.ISession;
        session.AllProfiles = [session.AllProfiles.First(x => x.Side != EPlayerSide.Savage), profile];
        session.ProfileOfPet.LearnAll();

        // Send scav profile to server so it knows of the items we might transfer
        RequestHandler.PutJson(
            "/raid/profile/scavsave",
            GetProfileAtEndOfRaidPatch.ProfileDescriptor.ToUnparsedData([]).JObject.ToString()
        );
    }
}
