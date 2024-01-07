using Aki.Debugging.BTR.Utils;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Aki.Debugging.BTR
{
    public class BTRManager : MonoBehaviour
    {
        private GameWorld gameWorld;
        private BotsController botsController;

        private BotBTRService btrBotService;
        private BTRControllerClass btrController;
        private BTRVehicle btrServerSide;
        private BTRView btrClientSide;
        private BTRDataPacket btrDataPacket = default;
        private bool btrBotShooterInitialized = false;

        private EPlayerBtrState previousPlayerBtrState;
        private BTRSide lastInteractedBtrSide;
        public BTRSide LastInteractedBtrSide => lastInteractedBtrSide;

        private MethodInfo _updateTaxiPriceMethod;

        private Dictionary<ETraderServiceType, Dictionary<string, bool>> ServicePurchasedDict { get; set; }

        BTRManager()
        {
            Type btrControllerType = typeof(BTRControllerClass);
            _updateTaxiPriceMethod = AccessTools.GetDeclaredMethods(btrControllerType).Single(IsUpdateTaxiPriceMethod);
            ServicePurchasedDict = new Dictionary<ETraderServiceType, Dictionary<string, bool>>();
        }

        // Find `BTRControllerClass.method_9(PathDestination currentDestinationPoint, bool lastRoutePoint)`
        private bool IsUpdateTaxiPriceMethod(MethodInfo method)
        {
            return (method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType == typeof(PathDestination));
        }

        public bool IsServicePurchased(ETraderServiceType serviceType, string traderId)
        {
            if (ServicePurchasedDict.TryGetValue(serviceType, out var traderDict))
            {
                if (traderDict.TryGetValue(traderId, out var result))
                {
                    return result;
                }
            }

            return false;
        }

        public void SetServicePurchased(ETraderServiceType serviceType, string traderId)
        {
            if (ServicePurchasedDict.TryGetValue(serviceType, out var traderDict))
            {
                traderDict[traderId] = true;
            }
            else
            {
                ServicePurchasedDict[serviceType] = new Dictionary<string, bool>();
                ServicePurchasedDict[serviceType][traderId] = true;
            }
        }

        private void Start()
        {
            try
            {
                gameWorld = Singleton<GameWorld>.Instance;

                if (gameWorld == null)
                {
                    Destroy(this);
                }

                if (gameWorld.BtrController == null)
                {
                    if (!Singleton<BTRControllerClass>.Instantiated)
                    {
                        Singleton<BTRControllerClass>.Create(new BTRControllerClass());
                    }

                    gameWorld.BtrController = btrController = Singleton<BTRControllerClass>.Instance;
                }

                botsController = Singleton<IBotGame>.Instance.BotsController;
                btrBotService = botsController.BotTradersServices.BTRServices;

                InitBTR();
            }
            catch
            {
                Debug.LogError("[AKI-BTR]: Unable to spawn BTR");
                DestroyGameObjects();
                throw;
            }
        }

        private void Update()
        {
            btrController.SyncBTRVehicleFromServer(UpdateDataPacket());

            // BotShooterBtr doesn't get assigned to BtrController immediately so we nullcheck this in Update
            if (btrController.BotShooterBtr != null && !btrBotShooterInitialized)
            {
                btrBotService.Reset(); // Player will be added to Neutrals list and removed from Enemies list
                btrBotShooterInitialized = true;
            }

            AimAtEnemy();
        }

        public void HandleBtrDoorState(EPlayerBtrState playerBtrState)
        {
            if (previousPlayerBtrState == EPlayerBtrState.Approach && playerBtrState == EPlayerBtrState.GoIn 
                || previousPlayerBtrState == EPlayerBtrState.Inside && playerBtrState == EPlayerBtrState.GoOut)
            {
                // Open Door
                UpdateBTRSideDoorState(1);
            }
            else if (previousPlayerBtrState == EPlayerBtrState.GoIn && playerBtrState == EPlayerBtrState.Inside 
                || previousPlayerBtrState == EPlayerBtrState.GoOut && playerBtrState == EPlayerBtrState.Outside)
            {
                // Close Door
                UpdateBTRSideDoorState(0);
            }

            previousPlayerBtrState = playerBtrState;
        }

        // Please tell me there's a better way than this xd
        public void OnPlayerInteractDoor(PlayerInteractPacket interactPacket)
        {
            var playerGoIn = interactPacket.InteractionType == EInteractionType.GoIn;
            var playerGoOut = interactPacket.InteractionType == EInteractionType.GoOut;

            if (interactPacket.SideId == 0)
            {
                if (interactPacket.SlotId == 0)
                {
                    if (playerGoIn) btrServerSide.LeftSlot0State = 1;
                    else if (playerGoOut) btrServerSide.LeftSlot0State = 0;
                }
                else if (interactPacket.SlotId == 1)
                {
                    if (playerGoIn) btrServerSide.LeftSlot1State = 1;
                    else if (playerGoOut) btrServerSide.LeftSlot1State = 0;
                }
            }
            else if (interactPacket.SideId == 1)
            {
                if (interactPacket.SlotId == 0)
                {
                    if (playerGoIn) btrServerSide.RightSlot0State = 1;
                    else if (playerGoOut) btrServerSide.RightSlot0State = 0;
                }
                else if (interactPacket.SlotId == 1)
                {
                    if (playerGoIn) btrServerSide.RightSlot1State = 1;
                    else if (playerGoOut) btrServerSide.RightSlot1State = 0;
                }
            }
        }

        private void InitBTR()
        {
            var btrControllerType = btrController.GetType();
            AccessTools.Method(btrControllerType, "method_3").Invoke(btrController, null); // spawns server-side BTR game object
            botsController.BotSpawner.SpawnBotBTR(); // spawns the scav bot which controls the BTR's turret

            btrServerSide = btrController.BtrVehicle;
            btrServerSide.moveSpeed = 20f;
            var btrMapConfig = btrController.MapPathsConfiguration;
            btrServerSide.CurrentPathConfig = btrMapConfig.PathsConfiguration.pathsConfigurations.RandomElement();
            btrServerSide.Initialization(btrMapConfig);
            AccessTools.Method(btrControllerType, "method_14").Invoke(btrController, null); // creates and assigns the BTR a fake stash

            DisableServerSideRenderers();

            gameWorld.MainPlayer.OnBtrStateChanged += HandleBtrDoorState;

            btrServerSide.MoveEnable();
            btrServerSide.IncomingToDestinationEvent += ToDestinationEvent;

            UpdateDataPacket();
            btrClientSide = btrController.BtrView;
            btrClientSide.transform.position = btrDataPacket.position;
            btrClientSide.transform.rotation = btrDataPacket.rotation;

            // Pull services data for the BTR from the server
            BTRUtil.PopulateTraderServicesData(BTRUtil.BTRTraderId);
        }

        /**
         * BTR has arrived at a destination, re-calculate taxi prices
         */
        private void ToDestinationEvent(PathDestination destinationPoint, bool isFirst, bool isFinal, bool isLastRoutePoint)
        {
            Debug.LogError($"[AKI-BTR] BTR arrived at destination {destinationPoint.id} isFirst: {isFirst} isFinal: {isFinal} isLastRoutePoint: {isLastRoutePoint}");

            // Update the prices for the taxi service
            _updateTaxiPriceMethod.Invoke(btrController, new object[] { destinationPoint, isFinal });
        }

        private void UpdateBTRSideDoorState(byte state)
        {
            var player = gameWorld.MainPlayer;
            var btrSides = (BTRSide[])AccessTools.Field(typeof(BTRView), "_btrSides").GetValue(btrController.BtrView);

            for (int i = 0; i < btrSides.Length; i++)
            {
                if (player.BtrInteractionSide != null && btrSides[i] == player.BtrInteractionSide 
                    || lastInteractedBtrSide != null && btrSides[i] == lastInteractedBtrSide)
                {
                    if (i == 0) btrServerSide.LeftSideState = state;
                    else if (i == 1) btrServerSide.RightSideState = state;

                    if (lastInteractedBtrSide != player.BtrInteractionSide)
                    {
                        lastInteractedBtrSide = player.BtrInteractionSide;
                    }
                }
            }
        }

        private BTRDataPacket UpdateDataPacket()
        {
            btrDataPacket.position = btrServerSide.transform.position;
            btrDataPacket.rotation = btrServerSide.transform.rotation;
            if (btrServerSide.BTRTurret?.gunsBlockRoot != null)
            {
                btrDataPacket.turretRotation = btrServerSide.BTRTurret.transform.rotation;
                btrDataPacket.gunsBlockRotation = btrServerSide.BTRTurret.gunsBlockRoot.rotation;
            }
            btrDataPacket.State = (byte)btrServerSide.BtrState;
            btrDataPacket.RouteState = (byte)btrServerSide.VehicleRouteState;
            btrDataPacket.LeftSideState = btrServerSide.LeftSideState;
            btrDataPacket.LeftSlot0State = btrServerSide.LeftSlot0State;
            btrDataPacket.LeftSlot1State = btrServerSide.LeftSlot1State;
            btrDataPacket.RightSideState = btrServerSide.RightSideState;
            btrDataPacket.RightSlot0State = btrServerSide.RightSlot0State;
            btrDataPacket.RightSlot1State = btrServerSide.RightSlot1State;
            btrDataPacket.currentSpeed = btrServerSide.currentSpeed;
            btrDataPacket.timeToEndPause = btrServerSide.timeToEndPause;
            btrDataPacket.moveDirection = (byte)btrServerSide.VehicleMoveDirection;
            btrDataPacket.MoveSpeed = btrServerSide.moveSpeed;
            if (btrController.BotShooterBtr != null)
            {
                btrDataPacket.BtrBotId = btrController.BotShooterBtr.Id;
            }

            return btrDataPacket;
        }

        private void DisableServerSideRenderers()
        {
            var meshRenderers = btrServerSide.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = false;
            }
        }

        private void AimAtEnemy()
        {
            if (btrController.BotShooterBtr == null)
            {
                return;
            }

            var btrTurret = btrServerSide.BTRTurret;
            var enemies = btrController.BotShooterBtr.BotsGroup.Enemies;
            if (enemies.Any())
            {
                btrTurret.EnableAimingObject(enemies.First().Key.Transform.Original);
            }
            else
            {
                btrTurret.DisableAiming();
            }
        }

        private void DestroyGameObjects()
        {
            if (btrController != null)
            {
                if (btrServerSide != null)
                {
                    Destroy(btrServerSide.gameObject);
                }
                if (btrClientSide != null)
                {
                    Destroy(btrClientSide.gameObject);
                }

                btrController.Dispose();
            }

            if (gameWorld?.MainPlayer != null)
            {
                gameWorld.MainPlayer.OnBtrStateChanged -= HandleBtrDoorState;
            }
            Destroy(this);
        }
    }
}
