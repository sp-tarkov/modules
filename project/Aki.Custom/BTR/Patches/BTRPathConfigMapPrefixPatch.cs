using System;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;

namespace Aki.Custom.BTR.Patches
{
    /// <summary>
    /// Fixes an issue where in a pathConfig.once type, finding destination path points was impossible because destinationID would be prefixed with "Map/", which the pathPoints do not contain.
    /// </summary>
    public class BTRPathConfigMapPrefixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(BTRControllerClass), IsTargetMethod);
        }

        private bool IsTargetMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            
            // BTRControllerClass.method_8
            return method.ReturnType == typeof(int)
                   && parameters.Length == 2
                   && parameters[0].ParameterType == typeof(string)
                   && parameters[0].Name == "destinationID"
                   && parameters[1].ParameterType == typeof(int)
                   && parameters[1].Name == "currentDestinationIndex";
        }

        [PatchPrefix]
        private static void PatchPrefix(ref string destinationID)
        {
            try
            {
                var locationIdSlash = Singleton<GameWorld>.Instance.LocationId + "/";

                if (destinationID.Contains(locationIdSlash))
                {
                    // destinationID is in the form of "Map/pX", strip the "Map/" part.
                    destinationID = destinationID.Replace(locationIdSlash, "");
                }
            }
            catch (Exception)
            {
                ConsoleScreen.LogError($"[AKI-BTR] Exception in {nameof(BTRPathConfigMapPrefixPatch)}, check logs.");
            }
        }
    }
}