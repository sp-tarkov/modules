using Aki.Common.Http;
using Aki.Reflection.Patching;
using EFT;
using Newtonsoft.Json;
using System;
using Comfort.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.PrePatch;
using Random = System.Random;
using EFT.InventoryLogic;

namespace Aki.Custom.Patches
{
    public class CustomAiPatch : ModulePatch
    {
        private static readonly string magazineId = "5448bc234bdc2d3c308b4569";
        private static readonly string drugId = "5448f3a14bdc2d27728b4569";
        private static readonly string mediKitItem = "5448f39d4bdc2d0a728b4568";
        private static readonly string medicalItemId = "5448f3ac4bdc2dce718b4569";
        private static readonly string injectorItemId = "5448f3a64bdc2d60728b456a";
        private static readonly string throwableItemId = "543be6564bdc2df4348b4568";
        private static readonly string ammoItemId = "5485a8684bdc2da71d8b4567";
        private static readonly string weaponId = "5422acb9af1c889c16000029";
        private static readonly List<string> nonFiRItems = new List<string>(){ magazineId , drugId, mediKitItem, medicalItemId, injectorItemId, throwableItemId, ammoItemId };

        private static readonly Random random = new Random();
        private static readonly Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>> botTypeCache = new Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>>();
        private static DateTime cacheDate = new DateTime();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(StandartBotBrain).GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Get a randomly picked wildspawntype from server and change PMC bot to use it, this ensures the bot is generated with that random type altering its behaviour
        /// Postfix will adjust it back to sptUsec/sptBear
        /// </summary>
        /// <param name="__state">state to save for postfix to use later</param>
        /// <param name="__instance"></param>
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPrefix]
        private static bool PatchPrefix(out WildSpawnType __state, StandartBotBrain __instance, BotOwner ___botOwner_0)
        {
            ___botOwner_0.Profile.Info.Settings.Role = FixAssaultGroupPmcsRole(___botOwner_0);
            __state = ___botOwner_0.Profile.Info.Settings.Role; // Store original type in state param to allow access in PatchPostFix()
            try
            {
                if (BotIsPlayerScav(__state, ___botOwner_0))
                {
                    ___botOwner_0.Profile.Info.Settings.Role = WildSpawnType.bossKilla;
                    return true; // Do original
                }

                if (BotIsSptPmc(__state, ___botOwner_0))
                {
                    if (___botOwner_0.Profile?.Inventory?.Equipment != null)
                    {
                        ConfigurePMCFindInRaidStatus(___botOwner_0);
                    }

                    if (!botTypeCache.TryGetValue(___botOwner_0.Profile.Info.Settings.Role, out var botSettings) || CacheIsStale())
                    {
                        ResetCacheDate();
                        HydrateCacheWithServerData();

                        if (!botTypeCache.TryGetValue(___botOwner_0.Profile.Info.Settings.Role, out botSettings))
                        {
                            throw new Exception($"Bots were refreshed from the server but the cache still doesnt contain an appropriate bot for type {___botOwner_0.Profile.Info.Settings.Role}");
                        }
                    }

                    string currentMapName = GetCurrentMap();
                    var mapSettings = botSettings[currentMapName.ToLower()];
                    var randomType = WeightedRandom(mapSettings.Keys.ToArray(), mapSettings.Values.ToArray());
                    if (Enum.TryParse(randomType, out WildSpawnType newAiType))
                    {
                        Logger.LogWarning($"Updated spt bot {___botOwner_0.Profile.Info.Nickname}: {___botOwner_0.Profile.Info.Settings.Role} to use: {newAiType} brain");
                        ___botOwner_0.Profile.Info.Settings.Role = newAiType;
                    }
                    else
                    {
                        Logger.LogError($"Couldnt not update spt bot {___botOwner_0.Profile.Info.Nickname} to random type {randomType}, does not exist for WildSpawnType enum");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error running CustomAiPatch PatchPrefix(): {ex.Message}");
                Logger.LogError(ex.StackTrace);
            }
            
            return true; // Do original 
        }

        /// <summary>
        /// the client sometimes replaces PMC roles with 'assaultGroup', give PMCs their original role back (sptBear/sptUsec)
        /// </summary>
        /// <returns>WildSpawnType</returns>
        private static WildSpawnType FixAssaultGroupPmcsRole(BotOwner botOwner)
        {
            if (botOwner.Profile.Info.IsStreamerModeAvailable && botOwner.Profile.Info.Settings.Role == WildSpawnType.assaultGroup)
            {
                Logger.LogError($"Broken PMC found: {botOwner.Profile.Nickname}, was {botOwner.Profile.Info.Settings.Role}");
                // Its a PMC, figure out what the bot originally was and return it
                return botOwner.Profile.Info.Side == EPlayerSide.Bear
                    ? (WildSpawnType)AkiBotsPrePatcher.sptBearValue
                    : (WildSpawnType)AkiBotsPrePatcher.sptUsecValue;
            }

            // Not broken pmc, return original role
            return botOwner.Profile.Info.Settings.Role;
        }

        private static void ConfigurePMCFindInRaidStatus(BotOwner ___botOwner_0)
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
                        //Logger.LogError($"Skipping item {item.Id} {item.Name} as its same as container {container.FullId}");
                        continue;
                    }

                    // Dont add FiR to tacvest items PMC usually brings into raid (meds/mags etc)
                    if (container.Name == "TacticalVest" && nonFiRItems.Any(item.Template._parent.Contains))
                    {
                        //Logger.LogError($"Skipping item {item.Id} {item.Name} as its on the item type blacklist");
                        continue;
                    }

                    // Don't add FiR to weapons in backpack (server sometimes adds pre-made weapons to backpack to simulate PMCs looting bodies)
                    if (container.Name == "Backpack" && new List<string> { weaponId }.Any(item.Template._parent.Contains))
                    {
                        //Logger.LogError($"Skipping item {item.Id} {item.Name} as its on the item type blacklist");
                        continue;
                    }

                    // Don't add FiR to grenades in pockets
                    if (container.Name == "Pockets" && new List<string> { throwableItemId, ammoItemId }.Any(item.Template._parent.Contains))
                    {
                        //Logger.LogError($"Skipping item {item.Id} {item.Name} as its on the item type blacklist");
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

        private static void MakeEquipmentNotFiR(BotOwner ___botOwner_0)
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

        /// <summary>
        /// Revert prefix change, get bots type back to what it was before changes
        /// </summary>
        /// <param name="__state">Saved state from prefix patch</param>
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPostfix]
        private static void PatchPostFix(WildSpawnType __state, BotOwner ___botOwner_0)
        {
            if (BotIsSptPmc(__state, ___botOwner_0))
            {
                // Set spt bot bot back to original type
                ___botOwner_0.Profile.Info.Settings.Role = __state;
            }
            else if (BotIsPlayerScav(__state, ___botOwner_0))
            {
                // Set pscav back to original type
                ___botOwner_0.Profile.Info.Settings.Role = __state;
            }
        }

        private static bool BotIsSptPmc(WildSpawnType role, BotOwner ___botOwner_0)
        {
            if (___botOwner_0.Profile.Info.IsStreamerModeAvailable)
            {
                // PMCs can sometimes have thier role changed to 'assaultGroup' by the client, we need a alternate way to figure out if they're a spt pmc
                return true;
            }

            return (int)role == AkiBotsPrePatcher.sptBearValue || (int)role == AkiBotsPrePatcher.sptUsecValue;
        }

        private static bool BotIsPlayerScav(WildSpawnType role, BotOwner ___botOwner_0)
        {
            if (___botOwner_0.Profile.Info.Nickname.Contains("(") && role == WildSpawnType.assault)
            {
                // Check bot is pscav by looking for the opening parentheses of their nickname e.g. scavname (pmc name)
                return true;
            }

            return false;
        }

        private static string GetCurrentMap()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            return gameWorld.MainPlayer.Location;
        }

        private static bool CacheIsStale()
        {
            TimeSpan cacheAge = DateTime.Now - cacheDate;

            return cacheAge.Minutes > 15;
        }

        private static void ResetCacheDate()
        {
            cacheDate = DateTime.Now;
            botTypeCache.Clear();
        }

        private static void HydrateCacheWithServerData()
        {
            // Get weightings for PMCs from server and store in dict
            var result = RequestHandler.GetJson($"/singleplayer/settings/bot/getBotBehaviours/");
            botTypeCache.AddRange(JsonConvert.DeserializeObject<Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>>>(result));
            Logger.LogWarning($"Cached bot.json/pmcType PMC brain weights in client");
        }

        /// <summary>
        /// Choose a value from a choice of values with weightings
        /// </summary>
        /// <param name="botTypes"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        private static string WeightedRandom(string[] botTypes, int[] weights)
        {
            var cumulativeWeights = new int[botTypes.Length];

            for (int i = 0; i < weights.Length; i++)
            {
                cumulativeWeights[i] = weights[i] + (i == 0 ? 0 : cumulativeWeights[i - 1]);
            }

            var maxCumulativeWeight = cumulativeWeights[cumulativeWeights.Length - 1];
            var randomNumber = maxCumulativeWeight * random.NextDouble();

            for (var itemIndex = 0; itemIndex < botTypes.Length; itemIndex++)
            {
                if (cumulativeWeights[itemIndex] >= randomNumber)
                {
                    return botTypes[itemIndex];
                }
            }

            Logger.LogError("failed to get random bot weighting, returned assault");

            return "assault";
        }
    }
}
