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
        private static List<string> BossConvertAllowedTypes = GetBossConvertFromServer();

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(StandartBotBrain), nameof(StandartBotBrain.Activate));
        }

        /// <summary>
        /// Get a randomly picked wildspawntype from server and change PMC bot to use it, this ensures the bot is generated with that random type altering its behaviour
        /// Postfix will adjust it back to original type
        /// </summary>
        /// <param name="__state">state to save for postfix to use later</param>
        /// <param name="__instance">StandartBotBrain</param>
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPrefix]
        private static bool PatchPrefix(out WildSpawnType __state, StandartBotBrain __instance, BotOwner ___botOwner_0)
        {
            ___botOwner_0.Profile.Info.Settings.Role = FixAssaultGroupPmcsRole(___botOwner_0);
            __state = ___botOwner_0.Profile.Info.Settings.Role; // Store original type in state param to allow access in PatchPostFix()
            try
            {
                string currentMapName = GetCurrentMap();
                var isPlayerScav = AiHelpers.BotIsPlayerScav(__state, ___botOwner_0.Profile.Info.Nickname);
                if (isPlayerScav)
                {
                    ___botOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetRandomisedPlayerScavType(___botOwner_0, currentMapName);

                    return true; // Do original
                }
                
                var isNormalAssaultScav = AiHelpers.BotIsNormalAssaultScav(__state, ___botOwner_0);
                if (isNormalAssaultScav)
                {
                    ___botOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetAssaultScavWildSpawnType(___botOwner_0, currentMapName);

                    return true; // Do original
                }

                var isSptPmc = AiHelpers.BotIsSptPmc(__state, ___botOwner_0);
                if (isSptPmc)
                {
                    // Bot has inventory equipment
                    if (___botOwner_0.Profile?.Inventory?.Equipment != null)
                    {
                        pmcFoundInRaidEquipment.ConfigurePMCFindInRaidStatus(___botOwner_0);
                    }

                    ___botOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetPmcWildSpawnType(___botOwner_0, ___botOwner_0.Profile.Info.Settings.Role, currentMapName);
                }

                // Is a boss bot and not already handled above
                if (BossConvertAllowedTypes.Contains(nameof(__state)))
                {
                    if (___botOwner_0.Boss.BossLogic == null)
                    {
                        // Ensure boss has AI init
                        Logger.LogError($"[SPT.CUSTOM] [CUSTOMAIPATCH] : bot: {___botOwner_0.Profile.Nickname} type: {___botOwner_0.Profile.Info.Settings.Role} lacked BossLogic, generating");
                        ___botOwner_0.Boss.SetBoss(0);
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

        /// <summary>
        /// the client sometimes replaces PMC roles with 'assaultGroup', give PMCs their original role back (pmcBEAR/pmcUSEC)
        /// </summary>
        /// <returns>WildSpawnType</returns>
        private static WildSpawnType FixAssaultGroupPmcsRole(BotOwner botOwner)
        {
            // Is PMC + set to assaultGroup
            if (botOwner.Profile.Info.IsStreamerModeAvailable && BotHasAssaultGroupRole(botOwner))
            {
                Logger.LogError($"Broken PMC found: {botOwner.Profile.Nickname}, was {botOwner.Profile.Info.Settings.Role}");

                // Its a PMC, figure out what the bot originally was and return it
                return botOwner.Profile.Info.Side == EPlayerSide.Bear
                    ? WildSpawnType.pmcBEAR
                    : WildSpawnType.pmcUSEC;
            }

            // Not broken pmc, return original role
            return botOwner.Profile.Info.Settings.Role;
        }

        private static List<string> GetBossConvertFromServer()
        {
            string json = RequestHandler.GetJson("/singleplayer/bossconvert");
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
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPostfix]
        private static void PatchPostFix(WildSpawnType __state, BotOwner ___botOwner_0)
        {
            if (AiHelpers.BotIsSptPmc(__state, ___botOwner_0))
            {
                // Set spt pmc bot back to original type
                ___botOwner_0.Profile.Info.Settings.Role = __state;
            }
            else if (AiHelpers.BotIsPlayerScav(__state, ___botOwner_0.Profile.Info.Nickname))
            {
                // Set pscav back to original type
                ___botOwner_0.Profile.Info.Settings.Role = __state;
            }
        }

        private static string GetCurrentMap()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            return gameWorld.MainPlayer.Location;
        }
    }
}
