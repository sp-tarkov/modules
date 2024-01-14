using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Progression;
using Comfort.Common;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.Progression
{
    public class LighthouseBridgePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null)
            {
                return;
            }

            if (gameWorld.MainPlayer.Location.ToLower() != "lighthouse" || gameWorld.MainPlayer.Side == EPlayerSide.Savage)
            {
                return;
            }

            gameWorld.GetOrAddComponent<LighthouseProgressionClass>();
        }        
    }
}