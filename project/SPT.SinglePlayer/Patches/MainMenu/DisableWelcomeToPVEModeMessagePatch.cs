using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

public class DisableWelcomeToPVEModeMessagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PrefsUtils), nameof(PrefsUtils.GetBoolForProfile));
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref bool __result, string variable)
    {
        if (variable == "pve_first_time")
        {
            __result = true;
            return false; // Skip original method
        }

        return true; // Do original method
    }
}
