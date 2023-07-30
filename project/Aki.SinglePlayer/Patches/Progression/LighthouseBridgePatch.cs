using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Progression;
using Comfort.Common;
using EFT;
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
            if (gameWorld == null || gameWorld.MainPlayer.Location.ToLower() != "lighthouse")
            {
                return;
            }

            gameWorld.GetOrAddComponent<LighthouseProgressionClass>();
        }
    }
}