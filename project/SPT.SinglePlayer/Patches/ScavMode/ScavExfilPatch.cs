using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.ScavMode;

/// <summary>
/// This patch return scav exfils if the player is playing as a scav by adding the player as eligible for the scav specific exfils
/// </summary>
public class ScavExfilPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(ExfiltrationControllerClass),
            nameof(ExfiltrationControllerClass.EligiblePoints),
            [typeof(Profile)]
        );
    }

    [PatchPrefix]
    public static bool PatchPrefix(Profile profile, ExfiltrationControllerClass __instance, ref ExfiltrationPoint[] __result)
    {
        if (profile.Info.Side != EPlayerSide.Savage)
        {
            return true; // Not a scav - don't do anything and run original method
        }

        // Running this prepares all the data for getting scav exfil points
        __instance.ScavExfiltrationClaim(
            ((IPlayer)Singleton<GameWorld>.Instance.MainPlayer).Position,
            profile.Id,
            profile.FenceInfo.AvailableExitsCount
        );

        // Get the required mask value and retrieve a list of exfil points, setting it as the result
        var mask = __instance.GetScavExfiltrationMask(profile.Id);
        __result = __instance.ScavExfiltrationClaim(mask, profile.Id);

        return false; // Don't run the original method anymore, as that will overwrite our new exfil points with ones meant for a PMC
    }
}
