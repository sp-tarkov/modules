using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using static GetActionsClass;

namespace SPT.Custom.Patches
{
    public class InsurancePlaceItem : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Class1492), nameof(Class1492.method_0));
        }

        [PatchPostfix]
        private static void PatchPostfix(Class1492 __instance, bool successful)
        {
            if (!successful)
            {
                return;
            }

            SinglePlayer.Utils.Insurance.InsuredItemManager.Instance.SetPlacedItem(__instance.resultItem);
        }
    }
}
