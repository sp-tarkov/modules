using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class PVEModeWelcomeMessagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1849), nameof(GClass1849.GetBoolForProfile));
        }
        
        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, string variable)
        {
            if (variable == "pve_first_time")
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}