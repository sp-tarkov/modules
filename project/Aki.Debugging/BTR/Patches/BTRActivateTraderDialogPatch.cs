using Aki.Debugging.BTR.Utils;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.UI.Screens;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using static EFT.UI.TraderDialogScreen;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRActivateTraderDialogPatch : ModulePatch
    {
        private static FieldInfo _playerInventoryControllerField;
        private static FieldInfo _playerQuestControllerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerInventoryControllerField = AccessTools.Field(typeof(Player), "_inventoryController");
            _playerQuestControllerField = AccessTools.Field(typeof(Player), "_questController");

            var targetType = typeof(GetActionsClass).GetNestedTypes(PatchConstants.PrivateFlags).Single(IsTargetType);
            return AccessTools.Method(targetType, "method_2");
        }

        private bool IsTargetType(Type type)
        {
            FieldInfo btrField = type.GetField("btr");

            if (btrField != null && btrField.FieldType == typeof(BTRSide))
            {
                return true;
            }

            return false;
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld.MainPlayer;

            InventoryControllerClass inventoryController = _playerInventoryControllerField.GetValue(player) as InventoryControllerClass;
            AbstractQuestControllerClass questController = _playerQuestControllerField.GetValue(player) as AbstractQuestControllerClass;

            GClass3130 btrDialog = new GClass3130(player.Profile, Profile.TraderInfo.TraderServiceToId[Profile.ETraderServiceSource.Btr], questController, inventoryController, null);
            btrDialog.OnClose += player.UpdateInteractionCast;
            btrDialog.ShowScreen(EScreenState.Queued);

            return false;
        }
    }
}
