using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;

namespace Aki.Debugging.BTR.Patches
{
    /// <summary>
    /// Adds a BTRManager component to the GameWorld game object when raid starts.
    /// </summary>
    public class BTRPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostfix()
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

                gameWorld.gameObject.AddComponent<BTRManager>();
            }
            catch (System.Exception)
            {
                ConsoleScreen.LogError("[AKI-BTR]: Exception thrown, check logs.");
                throw;
            }
        }
    }
}