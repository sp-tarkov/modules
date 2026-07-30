using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
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
        __instance._succeed = true;
        __result = Task.CompletedTask;
        return false; // Skip original
    }
}
