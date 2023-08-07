using Aki.SinglePlayer.Models.RaidFix;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;

namespace Aki.SinglePlayer.Utils.Insurance
{
    public class InsuredItemManager
    {
        private static InsuredItemManager _instance;
        private List<Item> _items;

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

        public List<AkiInsuredItemClass> GetTrackedItems()
        {
            var itemsToSend = new List<AkiInsuredItemClass>();
            if (_items == null || _items.Count() == 0)
            {
                return itemsToSend;
            }

            foreach (var item in _items)
            {
                var aki = new AkiInsuredItemClass
                {
                    id = item.Id
                };

                var dura = item.GetItemComponent<RepairableComponent>();

                if (dura != null)
                {
                    aki.durability = dura.Durability;
                    aki.maxDurability = dura.MaxDurability;
                }

                var faceshield = item.GetItemComponent<FaceShieldComponent>();

                if (faceshield != null)
                {
                    aki.hits = faceshield.Hits;
                }

                itemsToSend.Add(aki);
            }

            return itemsToSend;
        }
    }
}
