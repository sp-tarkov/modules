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
        public static bool PatchPrefix(Profile profile, ExfiltrationControllerClass __instance, ref ExfiltrationPoint[] __result)
        {
            if (profile.Info.Side != EPlayerSide.Savage)
            {
                return true; // Not a scav - don't do anything and run original method
            }

            if (__instance.ScavExfiltrationPoints.Length > 0)
            {
                Logger.LogError($"ScavExfiltrationPoints has content, Do original");
                foreach (var scavExit in __instance.ScavExfiltrationPoints)
                {
                    Logger.LogError($"{scavExit.name}, {scavExit.Id}, {scavExit.Description}");
                }

                return true; // do original
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