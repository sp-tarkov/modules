using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.UI.SessionEnd;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Progression
{
    public class ExperienceGainPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(SessionResultExperienceCount);
            var desiredMethod = desiredType.GetMethods(PatchConstants.PrivateFlags).FirstOrDefault(IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 3
                && parameters[0].Name == "profile"
                && parameters[1].Name == "isOnline"
                && parameters[2].Name == "exitStatus"
                && parameters[1].ParameterType == typeof(bool));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref bool isOnline)
        {
            isOnline = false;
        }

        [PatchPostfix]
        private static void PatchPostfix(ref bool isOnline)
        {
            isOnline = true;
        }
    }
}
