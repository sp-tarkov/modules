using Aki.Reflection.Patching;
using Aki.SinglePlayer.Utils.TraderServices;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Aki.Custom.Patches
{
    public class ResetTraderServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseLocalGame<GamePlayerOwner>), nameof(BaseLocalGame<GamePlayerOwner>.Stop));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            TraderServicesManager.Instance.Clear();
        }
    }
}
