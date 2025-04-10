using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// BSG in their wisdom on ctor set everything to certain values, this then runs and logs out an issue with Threshold durability,
/// this is then never able to log again.
/// we are sending the correct values on the server, so we can disable this
/// </summary>
public class DisableBuffLoggingPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BuffComponent), nameof(BuffComponent.method_0));
    }

    [PatchPrefix]
    public static void PatchPrefix(ref bool ___bool_0)
    {
        ___bool_0 = true;
    }
}