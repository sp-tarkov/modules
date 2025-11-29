using System.Reflection;
using System.Threading.Tasks;
using BattlEye;
using EFT;
using HarmonyLib;
using SPT.Core.Utils;
using SPT.Reflection.Patching;

namespace SPT.Core.Patches;

public class BattlEyePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(AnticheatValidationOperation), nameof(AnticheatValidationOperation.RunValidation));
    }

    [PatchPrefix]
    private static bool PatchPrefix(AnticheatValidationOperation __instance, ref Task __result)
    {
        __instance.bool_0 = ValidationUtil.Validate();
        __result = Task.CompletedTask;
        return false; // Skip original
    }
}
