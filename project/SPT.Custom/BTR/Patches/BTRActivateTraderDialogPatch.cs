using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI.Screens;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Reflection;
using BTRDialog = EFT.UI.TraderDialogScreen.GClass3156;

namespace SPT.Custom.BTR.Patches
{
    public class BTRActivateTraderDialogPatch : ModulePatch
    {
        private static FieldInfo _playerInventoryControllerField;
        private static FieldInfo _playerQuestControllerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerInventoryControllerField = AccessTools.Field(typeof(Player), "_inventoryController");
            _playerQuestControllerField = AccessTools.Field(typeof(Player), "_questController");

            var targetType = AccessTools.FirstInner(typeof(GetActionsClass), IsTargetType);
            return AccessTools.Method(targetType, "method_2");
        }

        private bool IsTargetType(Type type)
        {
            FieldInfo btrField = type.GetField("btr");

            return btrField != null && btrField.FieldType == typeof(BTRSide);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld.MainPlayer;

            InventoryControllerClass inventoryController = _playerInventoryControllerField.GetValue(player) as InventoryControllerClass;
            AbstractQuestControllerClass questController = _playerQuestControllerField.GetValue(player) as AbstractQuestControllerClass;

            BTRDialog btrDialog = new BTRDialog(player.Profile, Profile.TraderInfo.TraderServiceToId[Profile.ETraderServiceSource.Btr], questController, inventoryController, null);
            btrDialog.OnClose += player.UpdateInteractionCast;
            btrDialog.ShowScreen(EScreenState.Queued);

            return false;
        }
    }
}
