﻿using Aki.Custom.Airdrops.Models;
using Aki.Custom.Airdrops.Utils;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace Aki.Custom.Airdrops
{
    public class AirdropsManager : MonoBehaviour
    {
        private AirdropPlane airdropPlane;
        private AirdropBox airdropBox;
        private ItemFactoryUtil factory;
        public bool isFlareDrop;
        private AirdropParametersModel airdropParameters;

        public async void Awake()
        {
            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;

                if (gameWorld == null)
                {
                    Destroy(this);
                }

                airdropParameters = AirdropUtil.InitAirdropParams(gameWorld, isFlareDrop);

                if (!airdropParameters.AirdropAvailable)
                {
                    Destroy(this);
                    return;
                }
            }
            catch
            {
                Debug.LogError("[AKI-AIRDROPS]: Unable to get config from server, airdrop won't occur");
                Destroy(this);
                throw;
            }

            try
            {
                airdropPlane = await AirdropPlane.Init(
                    airdropParameters.RandomAirdropPoint,
                    airdropParameters.DropHeight,
                    airdropParameters.Config.PlaneVolume,
                    airdropParameters.Config.PlaneSpeed);
                airdropBox = await AirdropBox.Init(airdropParameters.Config.CrateFallSpeed);
                factory = new ItemFactoryUtil();
            }
            catch
            {
                Debug.LogError("[AKI-AIRDROPS]: Unable to create plane or crate, airdrop won't occur");
                Destroy(this);
                throw;
            }

            SetDistanceToDrop();
        }

        public void FixedUpdate()
        {
            if (airdropParameters == null || airdropPlane == null || airdropBox == null) return;

            try
            {
                airdropParameters.Timer += 0.02f;

                if (airdropParameters.Timer >= airdropParameters.TimeToStart && !airdropParameters.PlaneSpawned)
                {
                    StartPlane();
                }

                if (!airdropParameters.PlaneSpawned)
                {
                    return;
                }

                if (airdropParameters.DistanceTraveled >= airdropParameters.DistanceToDrop && !airdropParameters.BoxSpawned)
                {
                    StartBox();
                    BuildLootContainer(airdropParameters.Config);
                }

                if (airdropParameters.DistanceTraveled < airdropParameters.DistanceToTravel)
                {
                    airdropParameters.DistanceTraveled += Time.deltaTime * airdropParameters.Config.PlaneSpeed;
                    var distanceToDrop = airdropParameters.DistanceToDrop - airdropParameters.DistanceTraveled;
                    airdropPlane.ManualUpdate(distanceToDrop);
                }
                else
                {
                    Destroy(airdropPlane.gameObject);
                    Destroy(this);
                }
            }
            catch
            {
                Debug.LogError("[AKI-AIRDROPS]: An error occurred during the airdrop FixedUpdate process");
                Destroy(airdropBox.gameObject);
                Destroy(airdropPlane.gameObject);
                Destroy(this);
                throw;
            }
        }

        private void StartPlane()
        {
            airdropPlane.gameObject.SetActive(true);
            airdropParameters.PlaneSpawned = true;
        }

        private void StartBox()
        {
            airdropParameters.BoxSpawned = true;
            var pointPos = airdropParameters.RandomAirdropPoint;
            var dropPos = new Vector3(pointPos.x, airdropParameters.DropHeight, pointPos.z);
            airdropBox.gameObject.SetActive(true);
            airdropBox.StartCoroutine(airdropBox.DropCrate(dropPos));
        }

        private void BuildLootContainer(AirdropConfigModel config)
        {
            var lootData = factory.GetLoot();
            factory.BuildContainer(airdropBox.container, config, lootData.DropType);
            factory.AddLoot(airdropBox.container, lootData);
        }

        private void SetDistanceToDrop()
        {
            airdropParameters.DistanceToDrop = Vector3.Distance(
                new Vector3(airdropParameters.RandomAirdropPoint.x, airdropParameters.DropHeight, airdropParameters.RandomAirdropPoint.z),
                airdropPlane.transform.position);
        }
    }
}