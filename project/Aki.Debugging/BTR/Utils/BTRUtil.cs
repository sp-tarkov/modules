using Aki.Common.Http;
using Aki.Debugging.BTR.Models;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static BackendConfigSettingsClass;
using TraderServiceClass = GClass1789;

namespace Aki.Debugging.BTR.Utils
{
    public static class BTRUtil
    {
        public static readonly string BTRTraderId = Profile.TraderInfo.BTR_TRADER_ID;

        private static FieldInfo _traderAvailableServicesField = AccessTools.Field(typeof(Profile.TraderInfo), "_availableServices");

        static BTRUtil()
        {
            // Sanity checks for compile time failure in the event the GClass changes
            _ = nameof(TraderServiceClass.CanAfford);
            _ = nameof(TraderServiceClass.WasPurchasedInThisRaid);
        }

        /**
         * Populate the given trader's services with server data
         */
        public static void PopulateTraderServicesData(string traderId)
        {
            if (!GetGameObjects(out GameWorld gameWorld, out BTRManager btrManager, out Player player))
            {
                Debug.LogError("[AKI-BTR] PopulateTraderServicesData - Error fetching game objects");
                return;
            }

            if (!player.Profile.TradersInfo.TryGetValue(traderId, out Profile.TraderInfo traderInfo))
            {
                Debug.LogError("[AKI-BTR] PopulateTraderServicesData - Error fetching profile trader info");
                return;
            }

            string json = RequestHandler.GetJson($"/singleplayer/traderServices/getTraderServices/{traderId}");
            var traderServiceModels = JsonConvert.DeserializeObject<List<TraderServiceModel>>(json);

            Dictionary<ETraderServiceType, ServiceData> servicesData = Singleton<BackendConfigSettingsClass>.Instance.ServicesData;
            foreach (var traderServiceModel in traderServiceModels)
            {
                ServiceData serviceData;

                // Only populate trader services that don't exist yet
                // Note: This is required because otherwise we overwrite some values the client sets itself.
                //       Normally this state would be handled via the server I guess
                if (!servicesData.ContainsKey(traderServiceModel.ServiceType))
                {
                    TraderServiceClass traderService = new TraderServiceClass();
                    traderService.TraderId = traderId;
                    traderService.ServiceType = traderServiceModel.ServiceType;
                    traderService.ItemsToPay = new Dictionary<MongoID, int>();
                    if (traderServiceModel.ItemsToPay != null)
                    {
                        foreach (var item in traderServiceModel.ItemsToPay)
                        {
                            traderService.ItemsToPay[item.Key] = item.Value;
                        }
                    }

                    // SubServices seem to be populated dynamically in the client (For BTR taxi atleast), so we can just ignore it
                    // NOTE: For future reference, this is a dict of `item _tpl` to `quantity`.
                    traderService.SubServices = new Dictionary<string, int>();

                    // TODO: What is this used for? Maybe lightkeeper?
                    traderService.UniqueItems = new MongoID[] { };

                    // Convert our format to the backend settings format and store it
                    serviceData = new ServiceData(traderService);
                    servicesData[serviceData.ServiceType] = serviceData;

                    // Set the service as available
                    traderInfo.SetServiceAvailability(serviceData.ServiceType, true, false);
                }
            }

            UpdateTraderServices(traderId);
        }

        /**
         * Update the trader services for the given trader with new WasPurchased data
         */
        public static void UpdateTraderServices(string traderId)
        {
            if (!GetGameObjects(out GameWorld gameWorld, out BTRManager btrManager, out Player player))
            {
                Debug.LogError("[AKI-BTR] UpdateTraderServices - Error fetching game objects");
                return;
            }

            if (player.Profile.TradersInfo.TryGetValue(traderId, out Profile.TraderInfo traderInfo))
            {
                var traderServices = _traderAvailableServicesField.GetValue(traderInfo) as HashSet<ETraderServiceType>;
                foreach (var traderService in traderServices)
                {
                    // TODO: We should probably actually calculate this?
                    var CanAfford = true;

                    // Check whether we've purchased this service yet
                    var WasPurchasedInThisRaid = btrManager.IsServicePurchased(traderService, traderId);

                    // Update the affordable and WasPurchased flags for the service, for this trader
                    traderInfo.SetServiceAvailability(traderService, CanAfford, WasPurchasedInThisRaid);
                }
            }
        }

        /**
         * Fetch common game properties via out parameters
         * 
         * Return false if any value is null
         */
        public static bool GetGameObjects(out GameWorld gameWorld, out BTRManager btrManager, out Player player)
        {
            gameWorld = Singleton<GameWorld>.Instance;
            btrManager = gameWorld?.GetComponent<BTRManager>();
            player = gameWorld?.MainPlayer;

            if (gameWorld == null)
            {
                Debug.LogError($"[AKI-BTR]: GetGameObjects - GameWorld is null");
                return false;
            }
            
            if (btrManager == null)
            {
                Debug.LogError($"[AKI-BTR]: GetGameObjects - BTRManagerr is null");
                return false;
            }
            
            if (player == null)
            {
                Debug.LogError($"[AKI-BTR]: GetGameObjects - Player is null");
                return false;
            }

            return true;
        }
    }
}
