using System;
using SPT.Reflection.Patching;
using EFT;
using EFT.Counters;
using EFT.UI.SessionEnd;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.Progression
{
    /// <summary>
    /// Fix xp gained value being 0 after a scav raid
    /// </summary>
    public class ScavExperienceGainPatch : ModulePatch
    {
        /// <summary>
        /// Looking for SessionResultExitStatus Show() (private)
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(SessionResultExitStatus),
                nameof(SessionResultExitStatus.Show),
                new []{ typeof(Profile), typeof(PlayerVisualRepresentation), typeof(ESideType), typeof(ExitStatus), typeof(TimeSpan), typeof(ISession), typeof(bool) });
        }

        // Unused, but left here in case patch breaks and finding the intended method is difficult
        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 7
                && parameters[0].Name == "activeProfile"
                && parameters[1].Name == "lastPlayerState"
                && parameters[2].Name == "side"
                && parameters[3].Name == "exitStatus"
                && parameters[4].Name == "raidTime");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Profile activeProfile,ref EPlayerSide side)
        {
            if (activeProfile.Side == EPlayerSide.Savage)
            {
                side = EPlayerSide.Savage; // Also set side to correct value (defaults to usec/bear when playing as scav)
                int xpGainedInSession = activeProfile.Stats.Eft.SessionCounters.GetAllInt(new object[] { CounterTag.Exp });
                activeProfile.Stats.Eft.TotalSessionExperience = (int)(xpGainedInSession * activeProfile.Stats.Eft.SessionExperienceMult * activeProfile.Stats.Eft.ExperienceBonusMult);
            }

            return true; // Always do original method
        }
    }
}