using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using UnityEngine;
using BTRController = GClass2911;
using BTRDataPacket = GStruct378;

namespace Aki.Debugging.BTR
{
    public class BTRManager : MonoBehaviour
    {
        private GameWorld gameWorld;
        private BTRController btrController;
        private BTRVehicle serverSideBtr;
        private BTRView clientSideBtr;
        private BTRDataPacket btrDataPacket = default;
        private EPlayerBtrState previousPlayerBtrState;
        private BTRSide previousInteractedBtrSide;

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

                var btrControllerType = btrController.GetType();
                AccessTools.Method(btrControllerType, "method_3").Invoke(btrController, null); // spawns server-side BTR game object
                Singleton<IBotGame>.Instance.BotsController.BotSpawner.SpawnBotBTR(); // spawns the scav bot which controls the BTR's turret

                serverSideBtr = btrController.BtrVehicle;
                serverSideBtr.moveSpeed = 20f;
                var btrMapConfig = btrController.MapPathsConfiguration;
                serverSideBtr.CurrentPathConfig = btrMapConfig.PathsConfiguration.pathsConfigurations.RandomElement();
                serverSideBtr.Initialization(btrMapConfig);
                AccessTools.Method(btrControllerType, "method_14").Invoke(btrController, null); // creates and assigns the BTR a fake stash

                clientSideBtr = btrController.BtrView;

                UpdateDataPacket();
                clientSideBtr.transform.position = btrDataPacket.position;
                clientSideBtr.transform.rotation = btrDataPacket.rotation;

                DisableServerSideRenderers();

                previousPlayerBtrState = gameWorld.MainPlayer.BtrState;
                gameWorld.MainPlayer.OnBtrStateChanged += HandleBtrDoorState;
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
        public void OnPlayerInteractDoor(GStruct167 interactPacket)
        {
            var playerGoIn = interactPacket.InteractionType == EInteractionType.GoIn;
            var playerGoOut = interactPacket.InteractionType == EInteractionType.GoOut;

            if (interactPacket.SideId == 0)
            {
                if (interactPacket.SlotId == 0)
                {
                    if (playerGoIn) serverSideBtr.LeftSlot0State = 1;
                    else if (playerGoOut) serverSideBtr.LeftSlot0State = 0;
                }
                else if (interactPacket.SlotId == 1)
                {
                    if (playerGoIn) serverSideBtr.LeftSlot1State = 1;
                    else if (playerGoOut) serverSideBtr.LeftSlot1State = 0;
                }
            }
            else if (interactPacket.SideId == 1)
            {
                if (interactPacket.SlotId == 0)
                {
                    if (playerGoIn) serverSideBtr.RightSlot0State = 1;
                    else if (playerGoOut) serverSideBtr.RightSlot0State = 0;
                }
                else if (interactPacket.SlotId == 1)
                {
                    if (playerGoIn) serverSideBtr.RightSlot1State = 1;
                    else if (playerGoOut) serverSideBtr.RightSlot1State = 0;
                }
            }
        }

        private void UpdateBTRSideDoorState(byte state)
        {
            var player = gameWorld.MainPlayer;
            var btrSides = (BTRSide[])AccessTools.Field(typeof(BTRView), "_btrSides").GetValue(btrController.BtrView);

            for (int i = 0; i < btrSides.Length; i++)
            {
                if (player.BtrInteractionSide != null && btrSides[i] == player.BtrInteractionSide 
                    || previousInteractedBtrSide != null && btrSides[i] == previousInteractedBtrSide)
                {
                    if (i == 0) serverSideBtr.LeftSideState = state;
                    else if (i == 1) serverSideBtr.RightSideState = state;

                    if ((previousInteractedBtrSide != player.BtrInteractionSide && player.BtrInteractionSide != null) 
                        || previousInteractedBtrSide == null)
                    {
                        previousInteractedBtrSide = player.BtrInteractionSide;
                    }
                }
            }
        }

        private BTRDataPacket UpdateDataPacket()
        {
            btrDataPacket.position = serverSideBtr.transform.position;
            btrDataPacket.rotation = serverSideBtr.transform.rotation;
            if (serverSideBtr.BTRTurret?.gunsBlockRoot != null)
            {
                btrDataPacket.turretRotation = serverSideBtr.BTRTurret.transform.rotation;
                btrDataPacket.gunsBlockRotation = serverSideBtr.BTRTurret.gunsBlockRoot.rotation;
            }
            btrDataPacket.State = (byte)serverSideBtr.BtrState;
            btrDataPacket.RouteState = (byte)serverSideBtr.VehicleRouteState;
            btrDataPacket.LeftSideState = serverSideBtr.LeftSideState;
            btrDataPacket.LeftSlot0State = serverSideBtr.LeftSlot0State;
            btrDataPacket.LeftSlot1State = serverSideBtr.LeftSlot1State;
            btrDataPacket.RightSideState = serverSideBtr.RightSideState;
            btrDataPacket.RightSlot0State = serverSideBtr.RightSlot0State;
            btrDataPacket.RightSlot1State = serverSideBtr.RightSlot1State;
            btrDataPacket.currentSpeed = serverSideBtr.currentSpeed;
            btrDataPacket.timeToEndPause = serverSideBtr.timeToEndPause;
            btrDataPacket.moveDirection = (byte)serverSideBtr.VehicleMoveDirection;
            btrDataPacket.MoveSpeed = serverSideBtr.moveSpeed;
            if (btrController.BotShooterBtr != null)
            {
                btrDataPacket.BtrBotId = btrController.BotShooterBtr.Id;
            }

            return btrDataPacket;
        }

        private void DisableServerSideRenderers()
        {
            var meshRenderers = serverSideBtr.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = false;
            }
        }

        private void DestroyGameObjects()
        {
            if (btrController != null)
            {
                if (serverSideBtr != null)
                {
                    Destroy(serverSideBtr.gameObject);
                }
                if (clientSideBtr != null)
                {
                    Destroy(clientSideBtr.gameObject);
                }

                btrController.Dispose();
            }

            if (gameWorld != null)
            {
                gameWorld.MainPlayer.OnBtrStateChanged -= HandleBtrDoorState;
            }
            Destroy(this);
        }
    }
}
