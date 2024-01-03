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
        private BTRController btrController;
        private BTRVehicle serverSideBtr;
        private BTRView clientSideBtr;
        private BTRDataPacket btrDataPacket = default;


        private void Start()
        {
            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;

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
                serverSideBtr.Initialization(btrMapConfig);
                AccessTools.Method(btrControllerType, "method_14").Invoke(btrController, null); // creates and assigns the BTR a fake stash
                serverSideBtr.CurrentPathConfig = btrMapConfig.PathsConfiguration.pathsConfigurations.RandomElement();

                clientSideBtr = btrController.BtrView;

                UpdateDataPacket();
                clientSideBtr.transform.position = btrDataPacket.position;
                clientSideBtr.transform.rotation = btrDataPacket.rotation;

                DisableServerSideRenderers();
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
            Destroy(this);
        }
    }
}
