﻿using EFT.InventoryLogic;
using EFT;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Aki.Custom.CustomAI
{
    public class PmcFoundInRaidEquipment
    {
        private static readonly string magazineId = "5448bc234bdc2d3c308b4569";
        private static readonly string drugId = "5448f3a14bdc2d27728b4569";
        private static readonly string mediKitItem = "5448f39d4bdc2d0a728b4568";
        private static readonly string medicalItemId = "5448f3ac4bdc2dce718b4569";
        private static readonly string injectorItemId = "5448f3a64bdc2d60728b456a";
        private static readonly string throwableItemId = "543be6564bdc2df4348b4568";
        private static readonly string ammoItemId = "5485a8684bdc2da71d8b4567";
        private static readonly string weaponId = "5422acb9af1c889c16000029";
        private static readonly List<string> nonFiRItems = new List<string>() { magazineId, drugId, mediKitItem, medicalItemId, injectorItemId, throwableItemId, ammoItemId };
        
        private static readonly string pistolId = "5447b5cf4bdc2d65278b4567"; 
        private static readonly string smgId = "5447b5e04bdc2d62278b4567";
        private static readonly string assaultRifleId = "5447b5f14bdc2d61278b4567";
        private static readonly string assaultCarbineId = "5447b5fc4bdc2d87278b4567";
        private static readonly string shotgunId = "5447b6094bdc2dc3278b4567";
        private static readonly string marksmanRifleId = "5447b6194bdc2d67278b4567";
        private static readonly string sniperRifleId = "5447b6254bdc2dc3278b4568";
        private static readonly string machinegunId = "5447bed64bdc2d97278b4568";
        private static readonly string grenadeLauncherId = "5447bedf4bdc2d87278b4568";
        private static readonly string knifeId = "5447e1d04bdc2dff2f8b4567";

        private static readonly List<string> weaponTypeIds = new List<string>() { pistolId, smgId, assaultRifleId, assaultCarbineId, shotgunId, marksmanRifleId, sniperRifleId, machinegunId, grenadeLauncherId, knifeId };
        private readonly ManualLogSource logger;

        public PmcFoundInRaidEquipment(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void ConfigurePMCFindInRaidStatus(BotOwner ___botOwner_0)
        {
            // Must run before the container loot code, otherwise backpack loot is not FiR
            MakeEquipmentNotFiR(___botOwner_0);

            // Get inventory items that hold other items (backpack/rig/pockets)
            List<Slot> containerGear = ___botOwner_0.Profile.Inventory.Equipment.GetContainerSlots();
            foreach (var container in containerGear)
            {
                foreach (var item in container.ContainedItem.GetAllItems())
                {
                    // Skip items that match container (array has itself as an item)
                    if (item.Id == container.Items.FirstOrDefault().Id)
                    {
                        //this.logger.LogError($"Skipping item {item.Id} {item.Name} as its same as container {container.FullId}");
                        continue;
                    }

                    // Dont add FiR to tacvest items PMC usually brings into raid (meds/mags etc)
                    if (container.Name == "TacticalVest" && nonFiRItems.Any(item.Template._parent.Contains))
                    {
                        //this.logger.LogError($"Skipping item {item.Id} {item.Name} as its on the item type blacklist");
                        continue;
                    }

                    // Don't add FiR to weapons in backpack (server sometimes adds pre-made weapons to backpack to simulate PMCs looting bodies)
                    if (container.Name == "Backpack" && weaponTypeIds.Any(item.Template._parent.Contains))
                    {
                        //this.logger.LogError($"Skipping item {item.Id} {item.Name} as its on the item type blacklist");
                        continue;
                    }

                    // Don't add FiR to grenades/mags/ammo in pockets
                    if (container.Name == "Pockets" && new List<string> { throwableItemId, ammoItemId, magazineId, medicalItemId }.Any(item.Template._parent.Contains))
                    {
                        //this.logger.LogError($"Skipping item {item.Id} {item.Name} as its on the item type blacklist");
                        continue;
                    }

                    //Logger.LogError($"flagging item FiR: {item.Id} {item.Name} _parent: {item.Template._parent}");
                    item.SpawnedInSession = true;
                }
            }

            // Set dogtag as FiR
            var dogtag = ___botOwner_0.Profile.Inventory.GetItemsInSlots(new EquipmentSlot[] { EquipmentSlot.Dogtag });
            dogtag.FirstOrDefault().SpawnedInSession = true;
        }


        private void MakeEquipmentNotFiR(BotOwner ___botOwner_0)
        {
            var additionalItems = ___botOwner_0.Profile.Inventory.GetItemsInSlots(new EquipmentSlot[]
            {   EquipmentSlot.Backpack,
                EquipmentSlot.FirstPrimaryWeapon,
                EquipmentSlot.SecondPrimaryWeapon,
                EquipmentSlot.TacticalVest,
                EquipmentSlot.ArmorVest,
                EquipmentSlot.Scabbard,
                EquipmentSlot.Eyewear,
                EquipmentSlot.Headwear,
                EquipmentSlot.Earpiece,
                EquipmentSlot.ArmBand,
                EquipmentSlot.FaceCover,
                EquipmentSlot.Holster,
                EquipmentSlot.SecuredContainer
            });

            foreach (var item in additionalItems)
            {
                // Some items are null, probably because bot doesnt have that particular slot on them
                if (item == null)
                {
                    continue;
                }

                //Logger.LogError($"flagging item FiR: {item.Id} {item.Name} _parent: {item.Template._parent}");
                item.SpawnedInSession = false;
            }
        }

    }
}
