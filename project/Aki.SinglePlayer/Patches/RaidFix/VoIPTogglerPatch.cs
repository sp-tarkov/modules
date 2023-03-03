using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class VoIPTogglerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ForceMuteVoIPToggler).GetMethod("Awake", PatchConstants.PrivateFlags);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }
}