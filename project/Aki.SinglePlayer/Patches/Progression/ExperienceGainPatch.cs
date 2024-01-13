using Aki.Reflection.Patching;
using EFT.UI.SessionEnd;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.Progression
{
    public class ExperienceGainPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SessionResultExperienceCount), nameof(SessionResultExperienceCount.Show), new []{ typeof(Profile), typeof(bool), typeof(ExitStatus) });
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
