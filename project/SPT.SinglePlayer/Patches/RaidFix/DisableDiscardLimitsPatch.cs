using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

public class DisableDiscardLimitsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(
            typeof(Player.PlayerOwnerInventoryController),
            nameof(Player.PlayerOwnerInventoryController.HasDiscardLimits)
        );
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
