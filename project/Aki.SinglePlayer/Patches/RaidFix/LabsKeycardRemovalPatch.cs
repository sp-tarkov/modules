using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Patch to remove the Labs Access Card from player inventory upon entering Labs
    /// </summary>
    public class LabsKeycardRemovalPatch : ModulePatch
    {
        private const string LabsAccessCardTemplateId = "5c94bbff86f7747ee735c08f";

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld?.MainPlayer;

            if (gameWorld == null || player == null)
            {
                return;
            }

            if (gameWorld.MainPlayer.Location.ToLower() != "laboratory")
            {
                return;
            }

            var accessCardItem = player.Profile.Inventory.AllRealPlayerItems.FirstOrDefault(x => x.TemplateId == LabsAccessCardTemplateId);

            if (accessCardItem == null)
            {
                return;
            }

            var inventoryController = Traverse.Create(player).Field<InventoryControllerClass>("_inventoryController").Value;
            GClass2768.Remove(accessCardItem, inventoryController, false, true);
        }
    }
}