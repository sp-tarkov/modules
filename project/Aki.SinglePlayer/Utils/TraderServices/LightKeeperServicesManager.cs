using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib.Tools;
using UnityEngine;

namespace Aki.SinglePlayer.Utils.TraderServices
{
    internal class LightKeeperServicesManager : MonoBehaviour
    {
        private static ManualLogSource logger;
        GameWorld gameWorld;
        BotsController botsController;

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LightKeeperServicesManager));

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || TraderServicesManager.Instance == null)
            {
                logger.LogError("[SPT-LKS] GameWorld or TraderServices null");
                Destroy(this);
                return;
            }

            botsController = Singleton<IBotGame>.Instance.BotsController;
            if (botsController == null)
            {
                logger.LogError("[SPT-LKS] BotsController null");
                Destroy(this);
                return;
            }

            TraderServicesManager.Instance.OnTraderServicePurchased += OnTraderServicePurchased;
        }

        private void OnTraderServicePurchased(ETraderServiceType serviceType, string subserviceId)
        {
            switch (serviceType)
            {
                case ETraderServiceType.ExUsecLoyalty:
                    botsController.BotTradersServices.LighthouseKeeperServices.OnFriendlyExUsecPurchased(gameWorld.MainPlayer);
                    break;
                case ETraderServiceType.ZryachiyAid:
                    botsController.BotTradersServices.LighthouseKeeperServices.OnFriendlyZryachiyPurchased(gameWorld.MainPlayer);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (gameWorld == null || botsController == null)
            {
                return;
            }

            if (TraderServicesManager.Instance != null)
            {
                TraderServicesManager.Instance.OnTraderServicePurchased -= OnTraderServicePurchased;
            }
        }
    }
}
