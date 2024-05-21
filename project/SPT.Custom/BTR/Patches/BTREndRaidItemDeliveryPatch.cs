using SPT.Common.Http;
using SPT.Custom.BTR.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace SPT.Custom.BTR.Patches
{
    public class BTREndRaidItemDeliveryPatch : ModulePatch
    {
        private static JsonConverter[] _defaultJsonConverters;

        protected override MethodBase GetTargetMethod()
        {
            var converterClass = typeof(AbstractGame).Assembly.GetTypes()
                .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            _defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;

            Type baseLocalGameType = PatchConstants.LocalGameType.BaseType;
            return AccessTools.Method(baseLocalGameType, nameof(LocalGame.Stop));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError("[SPT-BTR] BTREndRaidItemDeliveryPatch - GameWorld is null");
                return;
            }
            var player = gameWorld.MainPlayer;
            if (player == null)
            {
                Logger.LogError("[SPT-BTR] BTREndRaidItemDeliveryPatch - Player is null");
                return;
            }

            // Match doesn't have a BTR
            if (gameWorld.BtrController == null)
            {
                return;
            }

            if (!gameWorld.BtrController.HasNonEmptyTransferContainer(player.Profile.Id))
            {
                Logger.LogDebug("[SPT-BTR] BTREndRaidItemDeliveryPatch - No items in transfer container");
                return;
            }

            var btrStash = gameWorld.BtrController.GetOrAddTransferContainer(player.Profile.Id);
            var flatItems = Singleton<ItemFactory>.Instance.TreeToFlatItems(btrStash.Grid.Items);

            RequestHandler.PutJson("/singleplayer/traderServices/itemDelivery", new
            {
                items = flatItems,
                traderId = BTRUtil.BTRTraderId
            }.ToJson(_defaultJsonConverters));
        }
    }
}
