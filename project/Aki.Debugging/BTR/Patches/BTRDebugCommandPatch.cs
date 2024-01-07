using Aki.Debugging.BTR.Utils;
using Aki.Reflection.Patching;
using EFT;
using EFT.UI;
using HarmonyLib;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    // Enable the `debug_show_dialog_screen` command
    internal class BTRDebugCommandPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ConsoleScreen), nameof(ConsoleScreen.InitConsole));
        }

        [PatchPostfix]
        internal static void PatchPostfix()
        {
            ConsoleScreen.Processor.RegisterCommandGroup<GClass1952>();
        }
    }

    // When running the `debug_show_dialog_screen` command, fetch the service data first, and force debug off
    public class BTRDebugDataPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1952), nameof(GClass1952.ShowDialogScreen));
        }

        [PatchPrefix]
        internal static void PatchPrefix(Profile.ETraderServiceSource traderServiceSourceType, ref bool useDebugData)
        {
            useDebugData = false;
            BTRUtil.PopulateTraderServicesData(Profile.TraderInfo.TraderServiceToId[traderServiceSourceType]);
        }
    }
}
