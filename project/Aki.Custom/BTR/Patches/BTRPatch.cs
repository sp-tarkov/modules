using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;

namespace Aki.Custom.BTR.Patches
{
    /// <summary>
    /// Adds a BTRManager component to the GameWorld game object when raid starts.
    /// </summary>
    public class BTRPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Note: This may seem like a weird place to hook, but `SetTime` requires that the BtrController 
            //       exist and be setup, so we'll use this as the entry point
            return AccessTools.Method(typeof(ExtractionTimersPanel), nameof(ExtractionTimersPanel.SetTime));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            try
            {
                var btrSettings = Singleton<BackendConfigSettingsClass>.Instance.BTRSettings;
                var gameWorld = Singleton<GameWorld>.Instance;

                // Only run on maps that have the BTR enabled
                string location = gameWorld.MainPlayer.Location;
                if (!btrSettings.LocationsWithBTR.Contains(location))
                {
                    return;
                }

                gameWorld.gameObject.AddComponent<BTRManager>();
            }
            catch (System.Exception)
            {
                ConsoleScreen.LogError("[SPT-BTR] Exception thrown, check logs.");
                throw;
            }
        }
    }
}