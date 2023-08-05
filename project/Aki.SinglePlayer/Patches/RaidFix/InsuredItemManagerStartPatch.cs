using Aki.Reflection.Patching;
using EFT;
using System.Reflection;
using Aki.SinglePlayer.Utils.Insurance;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class InsuredItemManagerStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            InsuredItemManager.Instance.Init();
        }
    }
}
