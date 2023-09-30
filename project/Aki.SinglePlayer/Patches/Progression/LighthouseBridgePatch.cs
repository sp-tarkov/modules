using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Progression;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Progression
{
    public class LighthouseBridgePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null)
            {
                return;
            }

            if (gameWorld.MainPlayer.Location.ToLower() != "lighthouse")
            {
                return;
            }

            gameWorld.GetOrAddComponent<LighthouseProgressionClass>();
        }        
    }
}