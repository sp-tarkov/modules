using SPT.Reflection.Patching;
using SPT.SinglePlayer.Utils.TraderServices;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace SPT.Custom.Patches
{
    public class ResetTraderServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseLocalGame<EftGamePlayerOwner>), nameof(BaseLocalGame<EftGamePlayerOwner>.Stop));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            TraderServicesManager.Instance.Clear();
        }
    }
}
