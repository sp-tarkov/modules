using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.UI.Screens;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using static EFT.UI.TraderDialogScreen;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRActivateTraderDialogPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetType = typeof(GClass1854).GetNestedTypes(PatchConstants.PrivateFlags).Where(x => x.Name == "Class1469").First();
            return AccessTools.Method(targetType, "method_2");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld.MainPlayer;

            var questController = (GClass3201)AccessTools.Field(player.GetType(), "_questController").GetValue(player);
            var inventoryController = (InventoryControllerClass)AccessTools.Field(player.GetType(), "_inventoryController").GetValue(player);

            GClass3130 btrDialog = new GClass3130(player.Profile, Profile.TraderInfo.BTR_TRADER_ID, questController, inventoryController, null);
            btrDialog.OnClose += player.UpdateInteractionCast;
            btrDialog.ShowScreen(EScreenState.Queued);
            //GClass1952.ShowDialogScreen(Profile.ETraderServiceSource.Btr, true);

            return false;
        }
    }
}
