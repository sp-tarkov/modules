using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using System;
using static LocationSettingsClass;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// Local games do not set the locationId property like a network game does, `LocationId` is used by various bsg systems
    /// e.g. btr/lightkeeper services
    /// </summary>
    public class SetLocationIdOnRaidStartPatch : ModulePatch
    {
        private static PropertyInfo _locationProperty;

        protected override MethodBase GetTargetMethod()
        {
            Type localGameBaseType = PatchConstants.LocalGameType.BaseType;

            // At this point, gameWorld.MainPlayer isn't set, so we need to use the LocalGame's `Location_0` property
            _locationProperty = localGameBaseType.GetProperties(PatchConstants.PrivateFlags).Single(x => x.PropertyType == typeof(Location));

            // Find the TimeAndWeatherSettings handling method
            return localGameBaseType.GetMethods(PatchConstants.PrivateFlags).SingleOrDefault(IsTargetMethod);
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            // Find method_3(TimeAndWeatherSettings timeAndWeather)
            var parameters = mi.GetParameters();
            return (parameters.Length == 1 && parameters[0].ParameterType == typeof(TimeAndWeatherSettings));
        }

        [PatchPostfix]
        private static void PatchPostfix(AbstractGame __instance)
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            // EFT.HideoutGame is an internal class, so we can't do static type checking :(
            if (__instance.GetType().Name.Contains("HideoutGame"))
            {
                return;
            }

            Location location = _locationProperty.GetValue(__instance) as Location;
            gameWorld.LocationId = location.Id;
        }
    }
}
