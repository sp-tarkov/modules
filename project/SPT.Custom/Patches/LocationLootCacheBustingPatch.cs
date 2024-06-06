using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System.Reflection;
using EFT;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// BaseLocalGame appears to cache a maps loot data and reuse it when the variantId from method_6 is the same, this patch exits the method early, never caching the data
    /// </summary>
    public class LocationLootCacheBustingPatch : ModulePatch
	{
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(BaseLocalGame<EftGamePlayerOwner>);
            var desiredMethod = desiredType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public).SingleCustom(IsTargetMethod); // method_6

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        // method_6
        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "backendUrl"
                && parameters[1].Name == "locationId"
                && parameters[2].Name == "variantId";
        }

        [PatchPrefix]
		private static bool PatchPrefix()
		{
            return false; // skip original
        }
    }
}
