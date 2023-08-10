using Aki.Common.Http;
using Aki.Reflection.Patching;
using EFT;
using Newtonsoft.Json;
using System;
using Comfort.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.PrePatch;
using Random = System.Random;

namespace Aki.Custom.Patches
{
    public class CustomAiPatch : ModulePatch
    {
        private static readonly Random random = new Random();
        private static Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>> botTypeCache = new Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>>();
        private static DateTime cacheDate = new DateTime();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(StandartBotBrain).GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Get a randomly picked wildspawntype from server and change PMC bot to use it, this ensures the bot is generated with that random type altering its behaviour
        /// </summary>
        /// <param name="__state">state to save for postfix to use later</param>
        /// <param name="__instance"></param>
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPrefix]
        private static bool PatchPrefix(out WildSpawnType __state, object __instance, BotOwner ___botOwner_0)
        {
            // Store original type in state param
            __state = ___botOwner_0.Profile.Info.Settings.Role;
            //Console.WriteLine($"Processing bot {___botOwner_0.Profile.Info.Nickname} with role {___botOwner_0.Profile.Info.Settings.Role}");
            try
            {
                if (BotIsSptPmc(___botOwner_0.Profile.Info.Settings.Role))
                {
                    string currentMapName = GetCurrentMap();

                    if (!botTypeCache.TryGetValue(___botOwner_0.Profile.Info.Settings.Role, out var botSettings) || CacheIsStale())
                    {
                        ResetCacheDate();
                        HydrateCacheWithServerData();

                        if (!botTypeCache.TryGetValue(___botOwner_0.Profile.Info.Settings.Role, out botSettings))
                        {
                            throw new Exception($"Bots were refreshed from the server but the cache still doesnt contain an appropriate bot for type {___botOwner_0.Profile.Info.Settings.Role}");
                        }
                    }

                    var mapSettings = botSettings[currentMapName.ToLower()];
                    var randomType = WeightedRandom(mapSettings.Keys.ToArray(), mapSettings.Values.ToArray());
                    if (Enum.TryParse(randomType, out WildSpawnType newAiType))
                    {
                        Logger.LogWarning($"Updated spt bot {___botOwner_0.Profile.Info.Nickname}: {___botOwner_0.Profile.Info.Settings.Role} to use: {newAiType} brain");
                        ___botOwner_0.Profile.Info.Settings.Role = newAiType;
                    }
                    else
                    {
                        Logger.LogError($"Couldnt not update spt bot {___botOwner_0.Profile.Info.Nickname} to random type {randomType}, does not exist for WildSpawnType enum");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing log: {ex.Message}");
                Logger.LogError(ex.StackTrace);
            }
            
            return true; // Do original 
        }

        /// <summary>
        /// Revert prefix change, get bots type back to what it was before changes
        /// </summary>
        /// <param name="__state">Saved state from prefix patch</param>
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPostfix]
        private static void PatchPostFix(WildSpawnType __state, BotOwner ___botOwner_0)
        {
            if (BotIsSptPmc(__state))
            {
                // Set spt bot bot back to original type
                ___botOwner_0.Profile.Info.Settings.Role = __state;
            }
        }

        private static bool BotIsSptPmc(WildSpawnType role)
        {
            return (int)role == AkiBotsPrePatcher.sptBearValue || (int)role == AkiBotsPrePatcher.sptUsecValue;
        }

        private static string GetCurrentMap()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            return gameWorld.MainPlayer.Location;
        }

        private static bool CacheIsStale()
        {
            TimeSpan cacheAge = DateTime.Now - cacheDate;

            return cacheAge.Minutes > 20;
        }

        private static void ResetCacheDate()
        {
            cacheDate = DateTime.Now;
        }

        private static void HydrateCacheWithServerData()
        {
            // Get weightings for PMCs from server and store in dict
            var result = RequestHandler.GetJson($"/singleplayer/settings/bot/getBotBehaviours/");
            botTypeCache = JsonConvert.DeserializeObject<Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>>>(result);
            Logger.LogWarning($"Cached bot.json/pmcType PMC brain weights in client");
        }

        private static string WeightedRandom(string[] botTypes, int[] weights)
        {
            var cumulativeWeights = new int[botTypes.Length];

            for (int i = 0; i < weights.Length; i++)
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

            Logger.LogError("failed to get random bot weighting, returned assault");

            return "assault";
        }
    }
}
