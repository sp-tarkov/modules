using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.Core.Patches
{
    public class WebSocketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetInterface = PatchConstants.EftTypes.Single(x => x == typeof(IConnectionHandler) && x.IsInterface);
            var typeThatMatches = PatchConstants.EftTypes.Single(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            return typeThatMatches.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.ReturnType == typeof(Uri));
        }

        [PatchPostfix]
        private static Uri PatchPostfix(Uri __instance)
        {
            return new Uri(__instance.ToString().Replace("wss:", "ws:"));
        }
    }
}
