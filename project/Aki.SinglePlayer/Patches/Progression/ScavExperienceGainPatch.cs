using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using EFT.Counters;
using EFT.UI.SessionEnd;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Progression
{
    public class ScavExperienceGainPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(SessionResultExitStatus);
            var desiredMethod = desiredType.GetMethods(PatchConstants.PrivateFlags).FirstOrDefault(IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

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
        private static bool PatchPrefix(ref Profile activeProfile,ref EPlayerSide side)
        {
            if (activeProfile.Side == EPlayerSide.Savage)
            {
                side = EPlayerSide.Savage;
                int allInt = activeProfile.Stats.SessionCounters.GetAllInt(new object[] { CounterTag.Exp });
                activeProfile.Stats.TotalSessionExperience = (int)((float)allInt * activeProfile.Stats.SessionExperienceMult * activeProfile.Stats.ExperienceBonusMult);
            }
            return true;
        }
    }
}