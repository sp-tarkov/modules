using SPT.Custom.BTR.Utils;
using SPT.SinglePlayer.Utils.TraderServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.Vehicle;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SPT.Custom.BTR
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
        private bool btrInitialized = false;
        private bool btrBotShooterInitialized = false;

        private float coverFireTime = 90f;
        private Coroutine _coverFireTimerCoroutine;

        private BTRSide lastInteractedBtrSide;
        public BTRSide LastInteractedBtrSide => lastInteractedBtrSide;

        private Coroutine _shootingTargetCoroutine;
        private BTRTurretServer btrTurretServer;
        private bool isTurretInDefaultRotation;
        private EnemyInfo currentTarget = null;
        private bool isShooting = false;
        private float machineGunAimDelay = 0.4f;
        private Vector2 machineGunBurstCount;
        private Vector2 machineGunRecoveryTime;
        private BulletClass btrMachineGunAmmo;
        private Item btrMachineGunWeapon;
        private Player.FirearmController firearmController;
        private WeaponSoundPlayer weaponSoundPlayer;

        private float originalDamageCoeff;

        private async void Awake()
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
                    gameWorld.BtrController = new BTRControllerClass();
                }

                btrController = gameWorld.BtrController;

                await InitBtr();
            }
            catch
            {
                ConsoleScreen.LogError("[SPT-BTR] Unable to spawn BTR. Check logs.");
                Destroy(this);
                throw;
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

            // If the player is going into the BTR, store their damage coefficient
            // and set it to 0, so they don't die while inside the BTR
            if (interactPacket.InteractionType == EInteractionType.GoIn)
            {
                originalDamageCoeff = gameWorld.MainPlayer.ActiveHealthController.DamageCoeff;
                gameWorld.MainPlayer.ActiveHealthController.SetDamageCoeff(0f);

            }
            // Otherwise restore the damage coefficient
            else if (interactPacket.InteractionType == EInteractionType.GoOut)
            {
                gameWorld.MainPlayer.ActiveHealthController.SetDamageCoeff(originalDamageCoeff);
            }
        }

        private void Update()
        {
            if (!btrInitialized) return;

            btrController.SyncBTRVehicleFromServer(UpdateDataPacket());

            if (btrController.BotShooterBtr == null) return;

            // BotShooterBtr doesn't get assigned to BtrController immediately so we check this in Update
            if (!btrBotShooterInitialized)
            {
                InitBtrBotService();
                btrBotShooterInitialized = true;
            }

            UpdateTarget();

            if (HasTarget())
            {
                SetAim();

                if (!isShooting && CanShoot())
                {
                    StartShooting();
                }
            }
            else if (!isTurretInDefaultRotation)
            {
                btrTurretServer.DisableAiming();
            }
        }

        private async Task InitBtr()
        {
            // Initial setup
            await btrController.InitBtrController();

            botEventHandler = Singleton<BotEventHandler>.Instance;
            var botsController = Singleton<IBotGame>.Instance.BotsController;
            btrBotService = botsController.BotTradersServices.BTRServices;
            btrController.method_3(); // spawns server-side BTR game object
            botsController.BotSpawner.SpawnBotBTR(); // spawns the scav bot which controls the BTR's turret

            // Initial BTR configuration
            btrServerSide = btrController.BtrVehicle;
            btrClientSide = btrController.BtrView;
            btrServerSide.transform.Find("KillBox").gameObject.AddComponent<BTRRoadKillTrigger>();

            // Get config from server and initialise respective settings
            ConfigureSettingsFromServer();

            var btrMapConfig = btrController.MapPathsConfiguration;
            if (btrMapConfig == null)
            {
                ConsoleScreen.LogError($"{nameof(btrController.MapPathsConfiguration)}");
                return;
            }
            btrServerSide.CurrentPathConfig = btrMapConfig.PathsConfiguration.pathsConfigurations.RandomElement();
            btrServerSide.Initialization(btrMapConfig);
            btrController.method_14(); // creates and assigns the BTR a fake stash

            DisableServerSideObjects();

            gameWorld.MainPlayer.OnBtrStateChanged += HandleBtrDoorState;

            btrServerSide.MoveEnable();
            btrServerSide.IncomingToDestinationEvent += ToDestinationEvent;

            // Sync initial position and rotation
            UpdateDataPacket();
            btrClientSide.transform.position = btrDataPacket.position;
            btrClientSide.transform.rotation = btrDataPacket.rotation;

            // Initialise turret variables
            btrTurretServer = btrServerSide.BTRTurret;
            var btrTurretDefaultTargetTransform = (Transform)AccessTools.Field(btrTurretServer.GetType(), "defaultTargetTransform").GetValue(btrTurretServer);
            isTurretInDefaultRotation = btrTurretServer.targetTransform == btrTurretDefaultTargetTransform
                && btrTurretServer.targetPosition == btrTurretServer.defaultAimingPosition;
            btrMachineGunAmmo = (BulletClass)BTRUtil.CreateItem(BTRUtil.BTRMachineGunAmmoTplId);
            btrMachineGunWeapon = BTRUtil.CreateItem(BTRUtil.BTRMachineGunWeaponTplId);

            // Pull services data for the BTR from the server
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(BTRUtil.BTRTraderId);

            btrInitialized = true;
        }

        private void ConfigureSettingsFromServer()
        {
            var serverConfig = BTRUtil.GetConfigFromServer();

            btrServerSide.moveSpeed = serverConfig.MoveSpeed;
            btrServerSide.pauseDurationRange.x = serverConfig.PointWaitTime.Min;
            btrServerSide.pauseDurationRange.y = serverConfig.PointWaitTime.Max;
            btrServerSide.readyToDeparture = serverConfig.TaxiWaitTime;
            coverFireTime = serverConfig.CoverFireTime;
            machineGunAimDelay = serverConfig.MachineGunAimDelay;
            machineGunBurstCount = new Vector2(serverConfig.MachineGunBurstCount.Min, serverConfig.MachineGunBurstCount.Max);
            machineGunRecoveryTime = new Vector2(serverConfig.MachineGunRecoveryTime.Min, serverConfig.MachineGunRecoveryTime.Max);
        }

        private void InitBtrBotService()
        {
            btrBotShooter = btrController.BotShooterBtr;
            firearmController = btrBotShooter.GetComponent<Player.FirearmController>();
            var weaponPrefab = (WeaponPrefab)AccessTools.Field(firearmController.GetType(), "weaponPrefab_0").GetValue(firearmController);
            weaponSoundPlayer = weaponPrefab.GetComponent<WeaponSoundPlayer>();

            btrBotService.Reset(); // Player will be added to Neutrals list and removed from Enemies list
            TraderServicesManager.Instance.OnTraderServicePurchased += BtrTraderServicePurchased;
        }

        /**
         * BTR has arrived at a destination, re-calculate taxi prices and remove purchased taxi service
         */
        private void ToDestinationEvent(PathDestination destinationPoint, bool isFirst, bool isFinal, bool isLastRoutePoint)
        {
            // Remove purchased taxi service
            TraderServicesManager.Instance.RemovePurchasedService(ETraderServiceType.PlayerTaxi, BTRUtil.BTRTraderId);

            // Update the prices for the taxi service
            btrController.UpdateTaxiPrice(destinationPoint, isFinal);

            // Update the UI
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(BTRUtil.BTRTraderId);
        }

        private bool IsBtrService(ETraderServiceType serviceType)
        {
            return serviceType == ETraderServiceType.BtrItemsDelivery
                || serviceType == ETraderServiceType.PlayerTaxi
                || serviceType == ETraderServiceType.BtrBotCover;
        }

        private void BtrTraderServicePurchased(ETraderServiceType serviceType, string subserviceId)
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
                    StartCoverFireTimer(coverFireTime);
                    break;
                case ETraderServiceType.PlayerTaxi:
                    btrController.BtrVehicle.IsPaid = true;
                    btrController.BtrVehicle.MoveToDestination(subserviceId.Split('/')[1]); // TODO: Look into fixing the main cause of this issue.
                    break;
            }
        }

        private void StartCoverFireTimer(float time)
        {
            _coverFireTimerCoroutine = StaticManager.BeginCoroutine(CoverFireTimer(time));
        }

        private IEnumerator CoverFireTimer(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            botEventHandler.StopTraderServiceBtrSupport();
        }

        private void HandleBtrDoorState(EPlayerBtrState playerBtrState)
        {
            if (playerBtrState == EPlayerBtrState.GoIn || playerBtrState == EPlayerBtrState.GoOut)
            {
                // Open Door
                UpdateBTRSideDoorState(1);
            }
            else if (playerBtrState == EPlayerBtrState.Inside || playerBtrState == EPlayerBtrState.Outside)
            {
                // Close Door
                UpdateBTRSideDoorState(0);
            }
        }

        private void UpdateBTRSideDoorState(byte state)
        {
            try
            {
                var player = gameWorld.MainPlayer;

                BTRSide btrSide = player.BtrInteractionSide != null ? player.BtrInteractionSide : lastInteractedBtrSide;
                byte sideId = btrClientSide.GetSideId(btrSide);
                switch (sideId)
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
            catch
            {
                ConsoleScreen.LogError($"[SPT-BTR] {nameof(lastInteractedBtrSide)} is null when it shouldn't be. Check logs.");
                throw;
            }
        }

        private BTRDataPacket UpdateDataPacket()
        {
            btrDataPacket.position = btrServerSide.transform.position;
            btrDataPacket.rotation = btrServerSide.transform.rotation;
            if (btrTurretServer != null && btrTurretServer.gunsBlockRoot != null)
            {
                btrDataPacket.turretRotation = btrTurretServer.transform.localEulerAngles.y;
                btrDataPacket.gunsBlockRotation = btrTurretServer.gunsBlockRoot.localEulerAngles.x;
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
            if (btrController != null && btrController.BotShooterBtr != null)
            {
                btrDataPacket.BtrBotId = btrController.BotShooterBtr.Id;
            }

            return btrDataPacket;
        }

        private void DisableServerSideObjects()
        {
            var meshRenderers = btrServerSide.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = false;
            }

            btrServerSide.turnCheckerObject.GetComponent<Renderer>().enabled = false; // Disables the red debug sphere

            // Something is colliding with each other. We disabled the Main exterior collider on server objects
            // and changed the layer of the Client exterior collider to be highPolyCollider to stop it twerking. Needs Proper fix
            var servercolliders = btrServerSide.transform.GetComponentsInChildren<MeshCollider>();
            var clientcolliders = btrClientSide.transform.GetComponentsInChildren<MeshCollider>();

            clientcolliders.FirstOrDefault(x => x.gameObject.name == "BTR_82_exterior_COLLIDER").gameObject.layer = LayerMask.NameToLayer("HighPolyCollider");
            servercolliders.FirstOrDefault(x => x.gameObject.name == "BTR_82_exterior_COLLIDER").enabled = false;
        }

        private void UpdateTarget()
        {
            currentTarget = btrBotShooter.Memory.GoalEnemy;
        }

        private bool HasTarget()
        {
            return currentTarget != null;
        }

        private void SetAim()
        {
            if (currentTarget.IsVisible)
            {
                Vector3 targetPos = currentTarget.CurrPosition;
                Transform targetTransform = currentTarget.Person.Transform.Original;
                if (btrTurretServer.CheckPositionInAimingZone(targetPos) && btrTurretServer.targetTransform != targetTransform)
                {
                    btrTurretServer.EnableAimingObject(targetTransform);
                }
            }
            else
            {
                Vector3 targetLastPos = currentTarget.EnemyLastPositionReal;
                if (btrTurretServer.CheckPositionInAimingZone(targetLastPos)
                    && Time.time - currentTarget.PersonalLastSeenTime < 3f
                    && btrTurretServer.targetPosition != targetLastPos)
                {
                    btrTurretServer.EnableAimingPosition(targetLastPos);

                }
                else if (Time.time - currentTarget.PersonalLastSeenTime >= 3f && !isTurretInDefaultRotation)
                {
                    btrTurretServer.DisableAiming();
                }
            }
        }

        private bool CanShoot()
        {
            return currentTarget.IsVisible && btrBotShooter.BotBtrData.CanShoot();
        }

        private void StartShooting()
        {
            _shootingTargetCoroutine = StaticManager.BeginCoroutine(ShootMachineGun());
        }

        /// <summary>
        /// Custom method to make the BTR coaxial machine gun shoot.
        /// </summary>
        private IEnumerator ShootMachineGun()
        {
            isShooting = true;

            yield return new WaitForSecondsRealtime(machineGunAimDelay);
            if (currentTarget?.Person == null || currentTarget?.IsVisible == false || !btrBotShooter.BotBtrData.CanShoot())
            {
                isShooting = false;
                yield break;
            }

            Transform machineGunMuzzle = btrTurretServer.machineGunLaunchPoint;
            var ballisticCalculator = gameWorld.SharedBallisticsCalculator;

            int burstMin = Mathf.FloorToInt(machineGunBurstCount.x);
            int burstMax = Mathf.FloorToInt(machineGunBurstCount.y);
            int burstCount = Random.Range(burstMin, burstMax + 1);
            Vector3 targetHeadPos = currentTarget.Person.PlayerBones.Head.position;
            while (burstCount > 0)
            {
                // Only update shooting position if the target isn't null
                if (currentTarget?.Person != null)
                {
                    targetHeadPos = currentTarget.Person.PlayerBones.Head.position;
                }

                Vector3 aimDirection = Vector3.Normalize(targetHeadPos - machineGunMuzzle.position);
                ballisticCalculator.Shoot(btrMachineGunAmmo, machineGunMuzzle.position, aimDirection, btrBotShooter.ProfileId, btrMachineGunWeapon, 1f, 0);
                firearmController.PlayWeaponSound(weaponSoundPlayer, btrMachineGunAmmo, machineGunMuzzle.position, aimDirection, false);

                burstCount--;
                yield return new WaitForSecondsRealtime(0.092308f); // 650 RPM
            }

            float waitTime = Random.Range(machineGunRecoveryTime.x, machineGunRecoveryTime.y);
            yield return new WaitForSecondsRealtime(waitTime);

            isShooting = false;
        }

        private void OnDestroy()
        {
            if (gameWorld == null)
            {
                return;
            }

            StaticManager.KillCoroutine(ref _shootingTargetCoroutine);
            StaticManager.KillCoroutine(ref _coverFireTimerCoroutine);

            if (TraderServicesManager.Instance != null)
            {
                TraderServicesManager.Instance.OnTraderServicePurchased -= BtrTraderServicePurchased;
            }

            if (gameWorld.MainPlayer != null)
            {
                gameWorld.MainPlayer.OnBtrStateChanged -= HandleBtrDoorState;
            }

            if (btrClientSide != null)
            {
                Debug.LogWarning($"[SPT-BTR] {nameof(BTRManager)} - Destroying btrClientSide");
                Destroy(btrClientSide.gameObject);
            }

            if (btrServerSide != null)
            {
                Debug.LogWarning($"[SPT-BTR] {nameof(BTRManager)} - Destroying btrServerSide");
                Destroy(btrServerSide.gameObject);
            }
        }
    }
}
