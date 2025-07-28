using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Core.Patches
{
    internal class Patch4002 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProfileEndpointFactoryAbstractClass), nameof(ProfileEndpointFactoryAbstractClass.SendMetricsJson));
        }

        [PatchPrefix]
        public static bool Prefix(ref Task __result)
        {
            __result = Task.CompletedTask;
            return false;
        }
    }
}
