using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// Removes BSG's checkmark to use BSG servers instead of local hosted
/// Also Sets checkmark to false.if checkmark is somehow enabled it will default to false (local raid only)
/// </summary>
public class DisableUseBSGServersCheckbox : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationInfoPanel), nameof(LocationInfoPanel.method_1));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref UpdatableToggle ____onlineModeToggle)
    {
        ____onlineModeToggle.isOn = false;
        ____onlineModeToggle.transform.parent.gameObject.SetActive(false);
    }
}
