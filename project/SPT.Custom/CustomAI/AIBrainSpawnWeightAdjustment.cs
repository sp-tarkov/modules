using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace SPT.Custom.CustomAI
{
    public class AIBrainSpawnWeightAdjustment
    {
        private static AIBrains _aiBrainsCache;
        private static DateTime _aiBrainCacheDate;
        private static readonly Random random = new();
        private readonly ManualLogSource _logger;

        public AIBrainSpawnWeightAdjustment(ManualLogSource logger)
        {
            _logger = logger;
        }

        public WildSpawnType GetRandomisedPlayerScavType(BotOwner botOwner, string currentMapName)
        {
            // Get map brain weights from server and cache
            if (_aiBrainsCache == null || CacheIsStale())
            {
                ResetCacheDate();
                HydrateCacheWithServerData();

                if (!_aiBrainsCache!.playerScav.TryGetValue(currentMapName.ToLower(), out _))
                {
                    throw new Exception($"Bots were refreshed from the server but the assault cache still doesn't contain data");
                }
            }

            // Choose random weighted brain
            var randomType = WeightedRandom(_aiBrainsCache.playerScav[currentMapName.ToLower()].Keys.ToArray(), _aiBrainsCache.playerScav[currentMapName.ToLower()].Values.ToArray());
            if (Enum.TryParse(randomType, out WildSpawnType newAiType))
            {
                _logger.LogWarning($"Updated player scav bot to use: {newAiType} brain");
                return newAiType;
            }

            _logger.LogWarning($"Unable to update bot: {botOwner.Profile.Info.Nickname} {botOwner.Profile.Info.Settings.Role} to use: {newAiType}, using default");

            return WildSpawnType.assault;
        }

        public WildSpawnType GetAssaultScavWildSpawnType(BotOwner botOwner, string currentMapName)
        {
            // Get map brain weights from server and cache
            if (_aiBrainsCache == null || CacheIsStale())
            {
                ResetCacheDate();
                HydrateCacheWithServerData();

                if (!_aiBrainsCache!.assault.TryGetValue(currentMapName.ToLower(), out _))
                {
                    throw new Exception($"Bots were refreshed from the server but the assault cache still doesn't contain data");
                }
            }

            // Choose random weighted brain
            var randomType = WeightedRandom(_aiBrainsCache.assault[currentMapName.ToLower()].Keys.ToArray(), _aiBrainsCache.assault[currentMapName.ToLower()].Values.ToArray());
            if (Enum.TryParse(randomType, out WildSpawnType newAiType))
            {
                _logger.LogWarning($"Updated assault bot {botOwner.Profile.Info.Nickname} to use: {newAiType} brain");
                return newAiType;
            }

            _logger.LogWarning($"Unable to parse brain type: {randomType} for {botOwner.Profile.Info.Nickname}, using default");

            return WildSpawnType.assault;
        }

        public WildSpawnType GetPmcWildSpawnType(BotOwner botOwner_0, WildSpawnType pmcType, string currentMapName)
        {
            if (_aiBrainsCache == null || !_aiBrainsCache.pmc.TryGetValue(pmcType, out var botSettings) || CacheIsStale())
            {
                ResetCacheDate();
                HydrateCacheWithServerData();

                if (!_aiBrainsCache!.pmc.TryGetValue(pmcType, out botSettings))
                {
                    throw new Exception($"Bots were refreshed from the server but the cache still doesnt contain an appropriate bot for type {botOwner_0.Profile.Info.Settings.Role}");
                }
            }

            var mapSettings = botSettings[currentMapName.ToLower()];
            var randomType = WeightedRandom(mapSettings.Keys.ToArray(), mapSettings.Values.ToArray());
            if (Enum.TryParse(randomType, out WildSpawnType newAiType))
            {
                _logger.LogWarning($"Updated spt bot {botOwner_0.Profile.Info.Nickname}: {botOwner_0.Profile.Info.Settings.Role} to use: {newAiType} brain");

                return newAiType;
            }

            _logger.LogError($"Couldn't update spt bot: {botOwner_0.Profile.Info.Nickname} to random type: {randomType}, does not exist for WildSpawnType enum, defaulting to 'assault'");

            return WildSpawnType.assault;
        }

        private void HydrateCacheWithServerData()
        {
            // Get weightings for PMCs from server and store in dict
            var result = RequestHandler.GetJson($"/singleplayer/settings/bot/getBotBehaviours/");
            _aiBrainsCache = JsonConvert.DeserializeObject<AIBrains>(result);
            _logger.LogWarning($"Cached ai brain weights in client");
        }

        private void ResetCacheDate()
        {
            _aiBrainCacheDate = DateTime.Now;
            _aiBrainsCache?.pmc?.Clear();
            _aiBrainsCache?.assault?.Clear();
            _aiBrainsCache?.playerScav?.Clear();
        }

        /// <summary>
        /// Has the ai brain cache been around longer than 15 minutes
        /// </summary>
        /// <returns></returns>
        private static bool CacheIsStale()
        {
            TimeSpan cacheAge = DateTime.Now - _aiBrainCacheDate;

            return cacheAge.Minutes > 15;
        }

        /// <summary>
        /// poco structure of data sent by server
        /// </summary>
        public class AIBrains
        {
            public Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>> pmc { get; set; }
            public Dictionary<string, Dictionary<string, int>> assault { get; set; }
            public Dictionary<string, Dictionary<string, int>> playerScav { get; set; }
        }

        /// <summary>
        /// Choose a value from a choice of values with weightings
        /// </summary>
        /// <param name="botTypes"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        private string WeightedRandom(string[] botTypes, int[] weights)
        {
            var cumulativeWeights = new int[botTypes.Length];

            for (var i = 0; i < weights.Length; i++)
            {
                cumulativeWeights[i] = weights[i] + (i == 0 ? 0 : cumulativeWeights[i - 1]);
            }

            var maxCumulativeWeight = cumulativeWeights[cumulativeWeights.Length - 1];
            var randomNumber = maxCumulativeWeight * random.NextDouble();

            for (var itemIndex = 0; itemIndex < botTypes.Length; itemIndex++)
            {
                if (cumulativeWeights[itemIndex] >= randomNumber)
                {
                    return botTypes[itemIndex];
                }
            }

            _logger.LogError("failed to get random bot brain weighting, returned assault");

            return "assault";
        }
    }
}
