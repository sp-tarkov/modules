using Aki.Reflection.Patching;
using Aki.SinglePlayer.Utils.TraderServices;
using Comfort.Common;
using EFT;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.TraderServices
{
    public class LightKeeperServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld != null)
            {
                gameWorld.gameObject.AddComponent<LightKeeperServicesManager>();
            }
        }
    }
}
