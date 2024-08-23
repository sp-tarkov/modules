using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class PVEModeWelcomeMessagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1925), nameof(GClass1925.GetBoolForProfile));
        }
        
        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result, string variable)
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