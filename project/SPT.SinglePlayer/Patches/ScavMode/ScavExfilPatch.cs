using System.Reflection;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class ScavExfilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationControllerClass), nameof(ExfiltrationControllerClass.EligiblePoints), new[] { typeof(Profile) });
        }

        [PatchPrefix]
        private static bool PatchPrefix(Profile profile, ExfiltrationControllerClass __instance, ref ExfiltrationPoint[] __result)
        {
            if (profile.Info.Side != EPlayerSide.Savage)
            {
                return true; // Not a scav - don't do anything and run original method
            }

            // We are on a map with zero scav exfil points, enable all of them.
            if (__instance.ScavExfiltrationPoints.Length == 0)
            {
                __result = __instance.ExfiltrationPoints;
                return false;
            }
            
            // Running this prepares all the data for getting scav exfil points
            __instance.ScavExfiltrationClaim(((IPlayer)Singleton<GameWorld>.Instance.MainPlayer).Position, profile.Id, profile.FenceInfo.AvailableExitsCount);
            
            // Get the required mask value and retrieve a list of exfil points, setting it as the result
            var mask = __instance.GetScavExfiltrationMask(profile.Id);
            __result = __instance.ScavExfiltrationClaim(mask, profile.Id);

            return false; // Don't run the original method anymore, as that will overwrite our new exfil points with ones meant for a PMC
        }
    }
}