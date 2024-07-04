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
            return AccessTools.Method(typeof(Class1518), nameof(Class1518.method_0));
        }

        [PatchPostfix]
        private static void PatchPostfix(Class1518 __instance, bool successful)
        {
            if (!successful)
            {
                return;
            }

            SinglePlayer.Utils.Insurance.InsuredItemManager.Instance.SetPlacedItem(__instance.resultItem);
        }
    }
}
