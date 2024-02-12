using Aki.Common.Http;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BackendConfigSettingsClass;
using TraderServiceClass = GClass1791;

namespace Aki.SinglePlayer.Utils.TraderServices
{
    public class TraderServicesManager
    {
        /// <summary>
        /// Subscribe to this event to trigger trader service logic.
        /// </summary>
        public event Action<ETraderServiceType, string> OnTraderServicePurchased; 

        private static TraderServicesManager _instance;

        public static TraderServicesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TraderServicesManager();
                }

                return _instance;
            }
        }

        private Dictionary<ETraderServiceType, Dictionary<string, bool>> _servicePurchased { get; set; }
        private HashSet<string> _cachedTraders = new HashSet<string>();

        public TraderServicesManager()
        {
            _servicePurchased = new Dictionary<ETraderServiceType, Dictionary<string, bool>>();
        }

        public void Clear()
        {
            _servicePurchased.Clear();
            _cachedTraders.Clear();
        }

        public void GetTraderServicesDataFromServer(string traderId)
        {
            Dictionary<ETraderServiceType, ServiceData> servicesData = Singleton<BackendConfigSettingsClass>.Instance.ServicesData;
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld?.MainPlayer;

            if (gameWorld == null || player == null)
            {
                Debug.LogError("GetTraderServicesDataFromServer - Error fetching game objects");
                return;
            }

            if (!player.Profile.TradersInfo.TryGetValue(traderId, out Profile.TraderInfo traderInfo))
            {
                Debug.LogError("GetTraderServicesDataFromServer - Error fetching profile trader info");
                return;
            }

            // Only request data from the server if it's not already cached
            if (!_cachedTraders.Contains(traderId))
            {
                var json = RequestHandler.GetJson($"/singleplayer/traderServices/getTraderServices/{traderId}");
                var traderServiceModels = JsonConvert.DeserializeObject<List<TraderServiceModel>>(json);

                foreach (var traderServiceModel in traderServiceModels)
                {
                    ETraderServiceType serviceType = traderServiceModel.ServiceType;
                    ServiceData serviceData;

                    // Only populate trader services that don't exist yet
                    if (!servicesData.ContainsKey(traderServiceModel.ServiceType))
                    {
                        TraderServiceClass traderService = new TraderServiceClass();
                        traderService.TraderId = traderId;
                        traderService.ServiceType = serviceType;
                        traderService.UniqueItems = traderServiceModel.ItemsToReceive;
                        traderService.ItemsToPay = traderServiceModel.ItemsToPay;

                        // SubServices seem to be populated dynamically in the client (For BTR taxi atleast), so we can just ignore it
                        // NOTE: For future reference, this is a dict of `point id` to `price` for the BTR taxi
                        traderService.SubServices = new Dictionary<string, int>();

                        // Convert our format to the backend settings format and store it
                        serviceData = new ServiceData(traderService);
                        servicesData[serviceData.ServiceType] = serviceData;
                    }
                }

                _cachedTraders.Add(traderId);
            }

            // Update service availability
            foreach (var servicesDataPair in servicesData)
            {
                // Only update this trader's services
                if (servicesDataPair.Value.TraderId != traderId)
                {
                    continue;
                }

                // TODO: We should probably actually calculate this?
                var CanAfford = true;

                // Check whether we've purchased this service yet
                var traderService = servicesDataPair.Key;
                var WasPurchasedInThisRaid = IsServicePurchased(traderService, traderId);
                traderInfo.SetServiceAvailability(traderService, CanAfford, WasPurchasedInThisRaid);
            }
        }

        public void AfterPurchaseTraderService(ETraderServiceType serviceType, AbstractQuestControllerClass questController, string subServiceId = null)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld?.MainPlayer;

            if (gameWorld == null || player == null)
            {
                Debug.LogError("TryPurchaseTraderService - Error fetching game objects");
                return;
            }

            // Service doesn't exist
            if (!Singleton<BackendConfigSettingsClass>.Instance.ServicesData.TryGetValue(serviceType, out var serviceData))
            {
                return;
            }

            SetServicePurchased(serviceType, subServiceId, serviceData.TraderId);
        }

        public void SetServicePurchased(ETraderServiceType serviceType, string subserviceId, string traderId)
        {
            if (_servicePurchased.TryGetValue(serviceType, out var traderDict))
            {
                traderDict[traderId] = true;
            }
            else
            {
                _servicePurchased[serviceType] = new Dictionary<string, bool>();
                _servicePurchased[serviceType][traderId] = true;
            }

            if (OnTraderServicePurchased != null)
            {
                OnTraderServicePurchased.Invoke(serviceType, subserviceId);
            }
        }

        public void RemovePurchasedService(ETraderServiceType serviceType, string traderId)
        {
            if (_servicePurchased.TryGetValue(serviceType, out var traderDict))
            {
                traderDict[traderId] = false;
            }
        }

        public bool IsServicePurchased(ETraderServiceType serviceType, string traderId)
        {
            if (_servicePurchased.TryGetValue(serviceType, out var traderDict))
            {
                if (traderDict.TryGetValue(traderId, out var result))
                {
                    return result;
                }
            }

            return false;
        }
    }
}
