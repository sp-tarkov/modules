﻿using SPT.SinglePlayer.Models.RaidFix;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;
using EFT.UI;

namespace SPT.SinglePlayer.Utils.Insurance
{
    public class InsuredItemManager
    {
        private static InsuredItemManager _instance;
        private List<Item> _items;
        private List<string> _placedItems = new List<string>();

        public static InsuredItemManager Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new InsuredItemManager();
                }

                return _instance;
            }
        }

        public void Init()
        {
            _items = Singleton<GameWorld>.Instance?.MainPlayer?.Profile?.Inventory?.AllRealPlayerItems?.ToList();
        }

        public List<SPTInsuredItemClass> GetTrackedItems()
        {
            var itemsToSend = new List<SPTInsuredItemClass>();
            if (_items == null || !_items.Any())
            {
                return itemsToSend;
            }

            foreach (var item in _items)
            {
                var spt = new SPTInsuredItemClass
                {
                    id = item.Id
                };

                if (_placedItems.Contains(item.Id))
                {
                    spt.usedInQuest = true;
                }

                var dura = item.GetItemComponent<RepairableComponent>();

                if (dura != null)
                {
                    spt.durability = dura.Durability;
                    spt.maxDurability = dura.MaxDurability;
                }

                var faceshield = item.GetItemComponent<FaceShieldComponent>();

                if (faceshield != null)
                {
                    spt.hits = faceshield.Hits;
                }

                itemsToSend.Add(spt);
            }

            return itemsToSend;
        }

        public void SetPlacedItem(Item topLevelItem)
        {
            // Includes Parent and Children items
            foreach (var item in topLevelItem.GetAllItems())
            {
                _placedItems.Add(item.Id);
            }
        }
    }
}
