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
        private List<Item> items;
        private List<InsuredItemClass> insuredItems;

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
            var player = Singleton<GameWorld>.Instance?.MainPlayer;

            items = player?.Profile?.Inventory?.AllRealPlayerItems.ToList();
            insuredItems = player?.Profile?.InsuredItems.ToList();
        }

        public List<AkiInsuredItemClass> GetTrackedItems()
        {
            var itemsToSend = new List<AkiInsuredItemClass>();

            foreach (var item in items)
            {
                // if the item does not exist in the "insured items" from the profile, skip
                if (insuredItems.Exists(x => x.itemId == item.Id)) continue;

                var aki = new AkiInsuredItemClass();

                aki.id = item.Id;

                var dura = item.GetItemComponent<RepairableComponent>();

                if (dura != null)
                {
                    aki.durability = dura.Durability;
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
