using SPT.Reflection.Patching;
using HarmonyLib;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SPT.Custom.BTR.Patches
{
    public class BTRControllerInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(BTRControllerClass), IsTargetMethod);
        }

        private bool IsTargetMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return method.ReturnType == typeof(Task)
                && parameters.Length == 1
                && parameters[0].ParameterType == typeof(CancellationToken);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result)
        {
            // The BTRControllerClass constructor expects the original method to return a Task,
            // as it calls another method on said Task.
            __result = Task.CompletedTask;
            return false;
        }
    }
}
