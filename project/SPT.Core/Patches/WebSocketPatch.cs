using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace SPT.Core.Patches
{
    public class WebSocketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UriParamsClass), nameof(UriParamsClass.method_0));
        }

        // This is a pass through postfix and behaves a little differently than usual
        // https://harmony.pardeike.net/articles/patching-postfix.html#pass-through-postfixes
        [PatchPostfix]
        private static Uri PatchPostfix(Uri __result)
        {
            return new Uri(__result.ToString().Replace("wss:", "ws:"));
        }
    }
}
