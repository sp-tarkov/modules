﻿using Aki.Custom.BTR.Patches;
using Aki.Reflection.Patching;
using Aki.SinglePlayer.Utils.TraderServices;
using EFT;
using EFT.UI;
using HarmonyLib;
using System.Reflection;
using DialogControlClass = GClass1954;

namespace Aki.Debugging.Patches
{
    // Enable the `debug_show_dialog_screen` command, and custom `btr_deliver_items` command
    internal class BTRDebugCommandPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ConsoleScreen), nameof(ConsoleScreen.InitConsole));
        }

        [PatchPostfix]
        internal static void PatchPostfix()
        {
            ConsoleScreen.Processor.RegisterCommandGroup<DialogControlClass>();
            ConsoleScreen.Processor.RegisterCommand("btr_deliver_items", new System.Action(BtrDeliverItemsCommand));
        }

        // Custom command to force item extraction sending
        public static void BtrDeliverItemsCommand()
        {
            BTREndRaidItemDeliveryPatch.PatchPrefix();
        }
    }

    // When running the `debug_show_dialog_screen` command, fetch the service data first, and force debug off
    public class BTRDebugDataPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(DialogControlClass), nameof(DialogControlClass.ShowDialogScreen));
        }

        [PatchPrefix]
        internal static void PatchPrefix(Profile.ETraderServiceSource traderServiceSourceType, ref bool useDebugData)
        {
            useDebugData = false;
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(Profile.TraderInfo.TraderServiceToId[traderServiceSourceType]);
        }
    }
}
