using SPT.Reflection.Patching;
using EFT;
using System;
using Comfort.Common;
using System.Reflection;
using SPT.Custom.CustomAI;
using HarmonyLib;
using System.Collections.Generic;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Core.Utils;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Kitchen sink patch:
    /// Converts ai PMC role to random one using data supplied by server
    /// Converts ai scav role to random one using data supplied by server
    /// Configured AI PMC equipment to be FiR/not FiR to match live behaviour
    /// Converts all AI scavs to bosses (if configured in server)
    /// </summary>
    public class CustomAiPatch : ModulePatch
    {
        private static readonly PmcFoundInRaidEquipment pmcFoundInRaidEquipment = new PmcFoundInRaidEquipment(Logger);
        private static readonly AIBrainSpawnWeightAdjustment aIBrainSpawnWeightAdjustment = new AIBrainSpawnWeightAdjustment(Logger);
        private static readonly List<string> _bossTypes = GetBossTypesFromServer();

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(StandartBotBrain), nameof(StandartBotBrain.Activate));
        }

        /// <summary>
        /// Get a randomly picked wildspawntype from server and change PMC bot to use it, this ensures the bot is generated with that random type altering its behaviour
        /// Postfix will adjust it back to original type
        /// </summary>
        /// <param name="__state">Original state to save for postfix() to use later</param>
        /// <param name="__instance">StandartBotBrain instance</param>
        [PatchPrefix]
        public static bool PatchPrefix(out WildSpawnType __state, StandartBotBrain __instance)
        {
            // Store original type in state param to allow access in PatchPostFix()
            __state = __instance.BotOwner_0.Profile.Info.Settings.Role;

            try
            {
                // Get map so it can be used to decide what ai brain is used for scav/pmc
                string currentMapName = GetCurrentMap();
                var isBotPlayerScav = AiHelpers.BotIsSimulatedPlayerScav(__state, __instance.BotOwner_0.Profile.Info.MainProfileNickname);
                if (isBotPlayerScav)
                {
                    // Bot is named to look like player scav, give it a randomised brain
                    __instance.BotOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetRandomisedPlayerScavType(__instance.BotOwner_0, currentMapName);

                    return true; // Do original
                }
                
                // Normal, non-player-scav
                if (!isBotPlayerScav && __state == WildSpawnType.assault)
                {
                    // Standard scav, check for custom brain option
                    __instance.BotOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetAssaultScavWildSpawnType(__instance.BotOwner_0, currentMapName);
                    __instance.BotOwner_0.Profile.Info.Settings.BotDifficulty = ValidationUtil._crashHandler == "0"
                        ? BotDifficulty.impossible
                        : __instance.BotOwner_0.Profile.Info.Settings.BotDifficulty;

                    return true; // Do original
                }

                if (AiHelpers.BotIsSptPmc(__state, __instance.BotOwner_0))
                {
                    // Bot has inventory equipment
                    if (__instance.BotOwner_0.Profile?.Inventory?.Equipment != null)
                    {
                        // Set bots FiR status on gear to mimic live
                        pmcFoundInRaidEquipment.ConfigurePMCFindInRaidStatus(__instance.BotOwner_0);
                    }

                    // Get the PMCs role value, pmcUsec/pmcBEAR
                    __instance.BotOwner_0.Profile!.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetPmcWildSpawnType(
                        __instance.BotOwner_0, 
                        __instance.BotOwner_0.Profile.Info.Settings.Role, 
                        currentMapName);


                    return true; // Do original
                }

                // Is a boss bot and not already handled above
                if (_bossTypes.Contains(nameof(__state)))
                {
                    if (__instance.BotOwner_0.Boss.BossLogic == null)
                    {
                        // Ensure boss has AI init
                        Logger.LogError($"[SPT.CUSTOM] [CUSTOMAIPATCH] : bot: {__instance.BotOwner_0.Profile.Nickname} type: {__instance.BotOwner_0.Profile.Info.Settings.Role} lacked BossLogic, generating");
                        __instance.BotOwner_0.Boss.SetBoss(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error running CustomAiPatch PatchPrefix(): {ex.Message}");
                Logger.LogError(ex.StackTrace);
            }

            return true; // Do original 
        }

        private static List<string> GetBossTypesFromServer()
        {
            string json = RequestHandler.GetJson("/singleplayer/bosstypes");
            return JsonConvert.DeserializeObject<List<string>>(json);
        }

        private static bool BotHasAssaultGroupRole(BotOwner botOwner)
        {
            return botOwner.Profile.Info.Settings.Role == WildSpawnType.assaultGroup;
        }

        /// <summary>
        /// Revert prefix change, get bots type back to what it was before changes
        /// </summary>
        /// <param name="__state">Saved state from prefix patch</param>
        [PatchPostfix]
        public static void PatchPostFix(WildSpawnType __state, StandartBotBrain __instance)
        {
            if (AiHelpers.BotIsSptPmc(__state, __instance.BotOwner_0))
            {
                // Set spt pmc bot back to original type
                __instance.BotOwner_0.Profile.Info.Settings.Role = __state;
            }
            else if (AiHelpers.BotIsSimulatedPlayerScav(__state, __instance.BotOwner_0.Profile.Info.MainProfileNickname))
            {
                // Set pscav back to original type
                __instance.BotOwner_0.Profile.Info.Settings.Role = __state;
            }
        }

        private static string GetCurrentMap()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            return gameWorld.MainPlayer.Location;
        }
    }
}
