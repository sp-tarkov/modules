using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;

namespace Aki.Debugging.BTR.Patches
{
    /// <summary>
    /// Adds a BTRManager component to the GameWorld game object when raid starts.
    /// </summary>
    internal class BTRPatch : ModulePatch
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
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld.MainPlayer.Location.ToLower() != "tarkovstreets")
                {
                    // only run patch on streets
                    return;
                }

                if (gameWorld.LocationId.IsNullOrEmpty())
                {
                    // GameWorld's LocationId needs to be set otherwise BTR doesn't get spawned in automatically
                    gameWorld.LocationId = gameWorld.MainPlayer.Location;
                }

                var btrManager = gameWorld.gameObject.AddComponent<BTRManager>();
                btrManager.Init();
            }
            catch (System.Exception)
            {
                ConsoleScreen.LogError("[AKI-BTR]: Exception thrown, check logs.");
                throw;
            }
        }
    }
}