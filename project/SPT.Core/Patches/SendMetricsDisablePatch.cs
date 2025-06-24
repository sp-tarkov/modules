using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Core.Coop.Patches
{
    internal class SendMetricsDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Class310), nameof(Class310.SendMetricsJson));
        }

        [PatchPrefix]
        public static bool Prefix(ref Task __result)
        {
            __result = Task.CompletedTask;
            return false;
        }
    }
}
