using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// BaseLocalGame appears to cache a maps loot data and reuse it when the variantId from method_6 is the same, this patch randomises the id to make caching less common
    /// </summary>
    public class LocationLootCacheBustingPatch : ModulePatch
	{
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "LocalGame").BaseType; // BaseLocalGame
            var desiredMethod = desiredType.GetMethods(PatchConstants.PrivateFlags).Single(x => IsTargetMethod(x)); // method_6

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "backendUrl"
                && parameters[1].Name == "locationId"
                && parameters[2].Name == "variantId";
        }

        [PatchPrefix]
		private static void PatchPrefix(ref int variantId)
		{
            var rand = new Random();
            variantId = rand.Next(0, 100000);
            Logger.LogError($"Randomised variantId to:{variantId}");
        }
    }
}
