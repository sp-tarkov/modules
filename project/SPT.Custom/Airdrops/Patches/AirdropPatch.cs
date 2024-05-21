using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using System.Linq;
using System.Reflection;

namespace SPT.Custom.Airdrops.Patches
{
    public class AirdropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points)
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>();
            }
        }
    }
}