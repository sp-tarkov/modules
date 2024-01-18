﻿using Aki.Common.Http;
using Aki.Custom.Airdrops.Models;
using EFT;
using EFT.Airdrop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Aki.Custom.Airdrops.Utils
{
    public static class AirdropUtil
    {
        public static AirdropConfigModel GetConfigFromServer()
        {
            string json = RequestHandler.GetJson("/singleplayer/airdrop/config");
            return JsonConvert.DeserializeObject<AirdropConfigModel>(json);
        }

        public static int ChanceToSpawn(GameWorld gameWorld, AirdropConfigModel config, bool isFlare)
        {
            // Flare summoned airdrops are guaranteed
            if (isFlare)
            {
                return 100;
            }

            // Get players current location
            string playerLocation = gameWorld.MainPlayer.Location;

            int result = 0;
            switch (playerLocation.ToLower())
            {
                case "bigmap":
                    {
                        result = config.AirdropChancePercent.Bigmap;
                        break;
                    }
                case "interchange":
                    {
                        result = config.AirdropChancePercent.Interchange;
                        break;
                    }
                case "rezervbase":
                    {
                        result = config.AirdropChancePercent.Reserve;
                        break;
                    }
                case "shoreline":
                    {
                        result = config.AirdropChancePercent.Shoreline;
                        break;
                    }
                case "woods":
                    {
                        result = config.AirdropChancePercent.Woods;
                        break;
                    }
                case "lighthouse":
                    {
                        result = config.AirdropChancePercent.Lighthouse;
                        break;
                    }
                case "tarkovstreets":
                    {
                        result = config.AirdropChancePercent.TarkovStreets;
                        break;
                    }
                default:
                    Debug.LogError($"[AKI-AIRDROPS]: Map with name {playerLocation} not handled, defaulting spawn chance to 25%");
                    result = 25;
                    break;
            }

            return result;
        }

        private static bool ShouldAirdropOccur(int dropChance, List<AirdropPoint> airdropPoints)
        {
            return airdropPoints.Count > 0 && Random.Range(0, 100) <= dropChance;
        }

        public static AirdropParametersModel InitAirdropParams(GameWorld gameWorld, bool isFlare)
        {
            var serverConfig = GetConfigFromServer();
            var allAirdropPoints = LocationScene.GetAll<AirdropPoint>().ToList();
            var playerPosition = ((IPlayer)gameWorld.MainPlayer).Position;
            var flareAirdropPoints = new List<AirdropPoint>();
            var dropChance = ChanceToSpawn(gameWorld, serverConfig, isFlare);
            var flareSpawnRadiusDistance = 100f;

            if (isFlare && allAirdropPoints.Count > 0)
            {
                foreach (AirdropPoint point in allAirdropPoints)
                {
                    if (Vector3.Distance(playerPosition, point.transform.position) <= flareSpawnRadiusDistance)
                    {
                        flareAirdropPoints.Add(point);
                    }
                }
            }

            if (flareAirdropPoints.Count == 0 && isFlare)
            {
                Debug.LogError($"[AKI-AIRDROPS]: Airdrop called in by flare, Unable to find an airdropPoint within {flareSpawnRadiusDistance}m, defaulting to normal drop");
                flareAirdropPoints.Add(allAirdropPoints.OrderBy(_ => Guid.NewGuid()).FirstOrDefault());
            }

            return new AirdropParametersModel()
            {
                Config = serverConfig,
                AirdropAvailable = ShouldAirdropOccur(dropChance, allAirdropPoints),

                DistanceTraveled = 0f,
                DistanceToTravel = 8000f,
                Timer = 0,
                PlaneSpawned = false,
                BoxSpawned = false,

                DropHeight = Random.Range(serverConfig.PlaneMinFlyHeight, serverConfig.PlaneMaxFlyHeight),
                TimeToStart = isFlare
                    ? 5
                    : Random.Range(serverConfig.AirdropMinStartTimeSeconds, serverConfig.AirdropMaxStartTimeSeconds),

                RandomAirdropPoint = isFlare && allAirdropPoints.Count > 0
                    ? flareAirdropPoints.OrderBy(_ => Guid.NewGuid()).First().transform.position
                    : allAirdropPoints.OrderBy(_ => Guid.NewGuid()).First().transform.position
            };
        }
    }
}
