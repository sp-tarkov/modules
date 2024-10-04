using SPT.Core.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;

namespace SPT.Core.Patches
{
    public class BattlEyePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BattleeyePatchClass), nameof(BattleeyePatchClass.RunValidation));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, ref bool ___bool_0)
        {
            ___bool_0 = ValidationUtil.Validate();
            __result = Task.CompletedTask;
            return false; // Skip original
        }
    }
}
