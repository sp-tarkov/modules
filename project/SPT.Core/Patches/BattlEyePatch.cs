using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using SPT.Core.Utils;
using SPT.Reflection.Patching;

namespace SPT.Core.Patches
{
    public class BattlEyePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(BattleeyePatchClass),
                nameof(BattleeyePatchClass.RunValidation)
            );
        }

        [PatchPrefix]
        private static bool PatchPrefix(BattleeyePatchClass __instance, ref Task __result)
        {
            __instance.Bool_0 = ValidationUtil.Validate();
            __result = Task.CompletedTask;
            return false; // Skip original
        }
    }
}
