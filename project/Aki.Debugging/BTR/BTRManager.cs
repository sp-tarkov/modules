using Aki.Debugging.BTR.Utils;
using Aki.SinglePlayer.Utils.TraderServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using BotEventHandler = GClass595;

namespace Aki.Debugging.BTR
{
    public class BTRManager : MonoBehaviour
    {
        private GameWorld gameWorld;
        private BotEventHandler botEventHandler;

        private BotBTRService btrBotService;
        private BTRControllerClass btrController;
        private BTRVehicle btrServerSide;
        private BTRView btrClientSide;
        private BotOwner btrBotShooter;
        private BTRDataPacket btrDataPacket = default;
        private bool btrBotShooterInitialized = false;

        private EPlayerBtrState previousPlayerBtrState;
        private BTRSide lastInteractedBtrSide;
        public BTRSide LastInteractedBtrSide => lastInteractedBtrSide;

        private Coroutine _coverFireTimerCoroutine;
        private BTRTurretServer btrTurretServer;
        private Transform btrTurretDefaultTargetTransform;
        private Coroutine _shootingTargetCoroutine;
        private IPlayer currentTarget = null;
        private bool isShooting = false;
        private BulletClass btrMachineGunAmmo;
        private Item btrMachineGunWeapon;

        private MethodInfo _updateTaxiPriceMethod;

        BTRManager()
        {
            Type btrControllerType = typeof(BTRControllerClass);
            _updateTaxiPriceMethod = AccessTools.GetDeclaredMethods(btrControllerType).Single(IsUpdateTaxiPriceMethod);
        }

        // Find `BTRControllerClass.method_9(PathDestination currentDestinationPoint, bool lastRoutePoint)`
        private bool IsUpdateTaxiPriceMethod(MethodInfo method)
        {
            return (method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType == typeof(PathDestination));
        }

        private void Start()
        {
            try
            {
                gameWorld = Singleton<GameWorld>.Instance;

                if (gameWorld == null)
                {
                    Destroy(this);
                    return;
                }

                if (gameWorld.BtrController == null)
                {
                    if (!Singleton<BTRControllerClass>.Instantiated)
                    {
                        Singleton<BTRControllerClass>.Create(new BTRControllerClass());
                    }

                    gameWorld.BtrController = btrController = Singleton<BTRControllerClass>.Instance;
                }

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

            if (btrController.BotShooterBtr == null) return;

            // BotShooterBtr doesn't get assigned to BtrController immediately so we check this in Update
            if (!btrBotShooterInitialized)
            {
                btrBotShooter = btrController.BotShooterBtr;
                btrBotService.Reset(); // Player will be added to Neutrals list and removed from Enemies list
                TraderServicesManager.Instance.OnTraderServicePurchased += BTRTraderServicePurchased;
                btrBotShooterInitialized = true;
            }

            if (HasTarget() && IsAimingAtTarget() && !isShooting)
            {
                _shootingTargetCoroutine = StaticManager.BeginCoroutine(ShootTarget());
            }

            if (_coverFireTimerCoroutine != null && ShouldCancelCoverFireSupport())
            {
                CancelCoverFireSupport();
            }
        }

        public void OnPlayerInteractDoor(PlayerInteractPacket interactPacket)
        {
            btrServerSide.LeftSlot0State = 0;
            btrServerSide.LeftSlot1State = 0;
            btrServerSide.RightSlot0State = 0;
            btrServerSide.RightSlot1State = 0;

            bool playerGoIn = interactPacket.InteractionType == EInteractionType.GoIn;

            if (interactPacket.SideId == 0 && playerGoIn)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.LeftSlot0State = 1;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.LeftSlot1State = 1;
                }
            }
            else if (interactPacket.SideId == 1 && playerGoIn)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.RightSlot0State = 1;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.RightSlot1State = 1;
                }
            }
        }

        private void InitBTR()
        {
            // Initial setup
            botEventHandler = Singleton<BotEventHandler>.Instance;
            var botsController = Singleton<IBotGame>.Instance.BotsController;
            btrBotService = botsController.BotTradersServices.BTRServices;
            btrController.method_3(); // spawns server-side BTR game object
            botsController.BotSpawner.SpawnBotBTR(); // spawns the scav bot which controls the BTR's turret

            // Initial BTR configuration
            btrServerSide = btrController.BtrVehicle;
            btrServerSide.transform.Find("KillBox").gameObject.AddComponent<BTRRoadKillTrigger>();
            btrServerSide.moveSpeed = 20f;
            var btrMapConfig = btrController.MapPathsConfiguration;
            btrServerSide.CurrentPathConfig = btrMapConfig.PathsConfiguration.pathsConfigurations.RandomElement();
            btrServerSide.Initialization(btrMapConfig);
            btrController.method_14(); // creates and assigns the BTR a fake stash

            DisableServerSideRenderers();

            gameWorld.MainPlayer.OnBtrStateChanged += HandleBtrDoorState;

            btrServerSide.MoveEnable();
            btrServerSide.IncomingToDestinationEvent += ToDestinationEvent;

            // Sync initial position and rotation
            UpdateDataPacket();
            btrClientSide = btrController.BtrView;
            btrClientSide.transform.position = btrDataPacket.position;
            btrClientSide.transform.rotation = btrDataPacket.rotation;

            // Initialise turret variables
            btrTurretServer = btrServerSide.BTRTurret;
            btrTurretDefaultTargetTransform = (Transform)AccessTools.Field(btrTurretServer.GetType(), "defaultTargetTransform").GetValue(btrTurretServer);
            btrMachineGunAmmo = (BulletClass)BTRUtil.CreateItem(BTRUtil.BTRMachineGunAmmoTplId);
            btrMachineGunWeapon = BTRUtil.CreateItem(BTRUtil.BTRMachineGunWeaponTplId);

            // Pull services data for the BTR from the server
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(BTRUtil.BTRTraderId);
        }

        /**
         * BTR has arrived at a destination, re-calculate taxi prices
         */
        private void ToDestinationEvent(PathDestination destinationPoint, bool isFirst, bool isFinal, bool isLastRoutePoint)
        {
            // Update the prices for the taxi service
            _updateTaxiPriceMethod.Invoke(btrController, new object[] { destinationPoint, isFinal });
        }

        private bool IsBtrService(ETraderServiceType serviceType)
        {
            if (serviceType == ETraderServiceType.BtrItemsDelivery 
                || serviceType == ETraderServiceType.PlayerTaxi 
                || serviceType == ETraderServiceType.BtrBotCover)
            {
                return true;
            }

            return false;
        }

        private void BTRTraderServicePurchased(ETraderServiceType serviceType)
        {
            if (!IsBtrService(serviceType))
            {
                return;
            }

            List<Player> passengers = gameWorld.AllAlivePlayersList.Where(x => x.BtrState == EPlayerBtrState.Inside).ToList();
            List<int> playersToNotify = passengers.Select(x => x.Id).ToList();
            btrController.method_6(playersToNotify, serviceType); // notify BTR passengers that a service has been purchased

            switch (serviceType)
            {
                case ETraderServiceType.BtrBotCover:
                    botEventHandler.ApplyTraderServiceBtrSupport(passengers);
                    StartCoverFireTimer(90f);
                    break;
                case ETraderServiceType.PlayerTaxi:
                    break;
            }
        }

        private void StartCoverFireTimer(float time)
        {
            _coverFireTimerCoroutine = StaticManager.BeginCoroutine(CoverFireTimer(time));
        }

        private bool ShouldCancelCoverFireSupport()
        {
            var friendlyPlayersByBtrSupport = (List<Player>)AccessTools.Field(btrBotService.GetType(), "_friendlyPlayersByBtrSupport").GetValue(btrBotService);
            if (!friendlyPlayersByBtrSupport.Any())
            {
                return true;
            }

            return false;
        }

        private void CancelCoverFireSupport()
        {
            StaticManager.KillCoroutine(ref _coverFireTimerCoroutine);
            botEventHandler.StopTraderServiceBtrSupport();
        }

        private IEnumerator CoverFireTimer(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            botEventHandler.StopTraderServiceBtrSupport();
        }

        private void HandleBtrDoorState(EPlayerBtrState playerBtrState)
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

        private void UpdateBTRSideDoorState(byte state)
        {
            var player = gameWorld.MainPlayer;
            var btrSides = (BTRSide[])AccessTools.Field(typeof(BTRView), "_btrSides").GetValue(btrController.BtrView);

            for (int i = 0; i < btrSides.Length; i++)
            {
                if (player.BtrInteractionSide != null && btrSides[i] == player.BtrInteractionSide 
                    || lastInteractedBtrSide != null && btrSides[i] == lastInteractedBtrSide)
                {
                    switch (i)
                    {
                        case 0:
                            btrServerSide.LeftSideState = state;
                            break;
                        case 1:
                            btrServerSide.RightSideState = state;
                            break;
                    }

                    lastInteractedBtrSide = player.BtrInteractionSide;
                }
            }
        }

        private BTRDataPacket UpdateDataPacket()
        {
            btrDataPacket.position = btrServerSide.transform.position;
            btrDataPacket.rotation = btrServerSide.transform.rotation;
            if (btrTurretServer?.gunsBlockRoot != null)
            {
                btrDataPacket.turretRotation = btrTurretServer.transform.rotation;
                btrDataPacket.gunsBlockRotation = btrTurretServer.gunsBlockRoot.rotation;
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

        private bool HasTarget()
        {
            var enemies = btrBotShooter.BotsGroup.Enemies;
            if (enemies.Any())
            {
                currentTarget = enemies.First().Key;
                if (!currentTarget.HealthController.IsAlive)
                {
                    enemies.Remove(currentTarget);
                    currentTarget = null;
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool IsAimingAtTarget()
        {
            bool turretInDefaultRotation = btrTurretServer.targetTransform == btrTurretDefaultTargetTransform 
                && btrTurretServer.targetPosition == btrTurretServer.defaultAimingPosition;

            if (currentTarget != null)
            {
                Transform currentTargetTransform = currentTarget.Transform.Original;
                EnemyInfo currentTargetInfo = btrBotShooter.EnemiesController.EnemyInfos[currentTarget];

                if (currentTargetInfo.IsVisible)
                {
                    Vector3 currentTargetPosition = currentTargetTransform.position;
                    if (btrTurretServer.CheckPositionInAimingZone(currentTargetPosition))
                    {
                        if (btrTurretServer.targetTransform == currentTargetTransform && btrBotShooter.BotBtrData.CanShoot())
                        {
                            return true;
                        }

                        if (btrTurretServer.targetTransform != currentTargetTransform)
                        {
                            btrTurretServer.EnableAimingObject(currentTargetTransform);
                        }
                    }
                }
                // Turret will hold the angle where target was last seen for 3 seconds before resetting its rotation
                else if (btrTurretServer.targetPosition != currentTargetInfo.EnemyLastPosition && btrTurretServer.targetTransform != null)
                {
                    btrTurretServer.EnableAimingPosition(currentTargetInfo.EnemyLastPosition);
                }
                else if (currentTargetInfo.TimeLastSeen >= 3f && !turretInDefaultRotation)
                {
                    currentTarget = null;
                    btrTurretServer.DisableAiming();
                }
            }
            else if (!turretInDefaultRotation)
            {
                btrTurretServer.DisableAiming();
            }

            return false;
        }

        /// <summary>
        /// Custom method to make the BTR coaxial machine gun shoot.
        /// </summary>
        private IEnumerator ShootTarget()
        {
            isShooting = true;

            Transform machineGunMuzzle = btrTurretServer.machineGunLaunchPoint;
            Player.FirearmController firearmController = btrBotShooter.GetComponent<Player.FirearmController>();
            WeaponPrefab weaponPrefab = (WeaponPrefab)AccessTools.Field(firearmController.GetType(), "weaponPrefab_0").GetValue(firearmController);
            WeaponSoundPlayer weaponSoundPlayer = weaponPrefab.GetComponent<WeaponSoundPlayer>();

            int burstCount = Random.Range(5, 8);
            while (burstCount > 0)
            {
                gameWorld.SharedBallisticsCalculator.Shoot(btrMachineGunAmmo, machineGunMuzzle.position, machineGunMuzzle.forward, btrBotShooter.ProfileId, btrMachineGunWeapon, 1f, 0);
                firearmController.method_54(weaponSoundPlayer, btrMachineGunAmmo, machineGunMuzzle.position, machineGunMuzzle.forward, false);
                burstCount--;
                yield return new WaitForSecondsRealtime(0.092308f); // 650 RPM
            }

            float waitTime = Random.Range(0.8f, 1.7f); // 0.8 - 1.7 second pause between bursts
            yield return new WaitForSecondsRealtime(waitTime);
            
            isShooting = false;
        }

        private void OnDestroy()
        {
            DestroyGameObjects();
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

            if (TraderServicesManager.Instance != null)
            {
                TraderServicesManager.Instance.OnTraderServicePurchased -= BTRTraderServicePurchased;
            }

            StaticManager.KillCoroutine(ref _shootingTargetCoroutine);
            StaticManager.KillCoroutine(ref _coverFireTimerCoroutine);
            Destroy(this);
        }
    }
}
