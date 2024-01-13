using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System;
using System.Reflection;

namespace Aki.Core.Patches
{
    public class WebSocketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetInterface = PatchConstants.EftTypes.SingleCustom(x => x == typeof(IConnectionHandler) && x.IsInterface);
            var typeThatMatches = PatchConstants.EftTypes.SingleCustom(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            
            return typeThatMatches.GetMethods(BindingFlags.Public | BindingFlags.Instance).SingleCustom(x => x.ReturnType == typeof(Uri));
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
