using SPT.Reflection.Patching;
using SPT.SinglePlayer.Utils.TraderServices;
using Comfort.Common;
using EFT;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.TraderServices
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
