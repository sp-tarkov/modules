using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class VoIPTogglerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ForceMuteVoIPToggler), nameof(ForceMuteVoIPToggler.Awake));
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }
}