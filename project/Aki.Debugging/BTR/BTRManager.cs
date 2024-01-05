using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using BTRController = GClass2911;
using BTRDataPacket = GStruct378;
using PlayerInteractPacket = GStruct167;

namespace Aki.Debugging.BTR
{
    public class BTRManager : MonoBehaviour
    {
        private GameWorld gameWorld;
        private BotsController botsController;

        private BotBTRService btrBotService;
        private BTRController btrController;
        private BTRVehicle btrServerSide;
        private BTRView btrClientSide;
        private BTRDataPacket btrDataPacket = default;
        private bool btrBotShooterInitialized = false;

        private EPlayerBtrState previousPlayerBtrState;
        private BTRSide lastInteractedBtrSide;
        public BTRSide LastInteractedBtrSide => lastInteractedBtrSide;


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
                    if (!Singleton<BTRController>.Instantiated)
                    {
                        Singleton<BTRController>.Create(new BTRController());
                    }

                    gameWorld.BtrController = btrController = Singleton<BTRController>.Instance;
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

            Type localGameBaseType = PatchConstants.LocalGameType.BaseType;

            btrServerSide.MoveEnable();

            UpdateDataPacket();
            btrClientSide = btrController.BtrView;
            btrClientSide.transform.position = btrDataPacket.position;
            btrClientSide.transform.rotation = btrDataPacket.rotation;
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
