using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Remove BSG's check for Ref as the TraderID so we get Ref on the interm screen
    /// </summary>
    public class EnableRefIntermScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MerchantsList.Class2860), nameof(MerchantsList.Class2860.method_0));
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false; // Do not run original method
        }
    }
}