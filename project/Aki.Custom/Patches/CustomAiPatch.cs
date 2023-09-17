﻿using Aki.Reflection.Patching;
using EFT;
using System;
using Comfort.Common;
using System.Reflection;
using Aki.PrePatch;
using Aki.Custom.CustomAI;

namespace Aki.Custom.Patches
{
    public class CustomAiPatch : ModulePatch
    {
        private static readonly PmcFoundInRaidEquipment pmcFoundInRaidEquipment = new PmcFoundInRaidEquipment();
        private static readonly AIBrainSpawnWeightAdjustment aIBrainSpawnWeightAdjustment = new AIBrainSpawnWeightAdjustment(Logger);

        protected override MethodBase GetTargetMethod()
        {
            return typeof(StandartBotBrain).GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Get a randomly picked wildspawntype from server and change PMC bot to use it, this ensures the bot is generated with that random type altering its behaviour
        /// Postfix will adjust it back to original type
        /// </summary>
        /// <param name="__state">state to save for postfix to use later</param>
        /// <param name="__instance"></param>
        /// <param name="___botOwner_0">botOwner_0 property</param>
        [PatchPrefix]
        private static bool PatchPrefix(out WildSpawnType __state, StandartBotBrain __instance, BotOwner ___botOwner_0)
        {
            ___botOwner_0.Profile.Info.Settings.Role = FixAssaultGroupPmcsRole(___botOwner_0);
            __state = ___botOwner_0.Profile.Info.Settings.Role; // Store original type in state param to allow access in PatchPostFix()
            try
            {
                if (AiHelpers.BotIsPlayerScav(__state, ___botOwner_0))
                {
                    ___botOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetRandomisedPlayerScavType();

                    return true; // Do original
                }
                string currentMapName = GetCurrentMap();
                if (AiHelpers.BotIsNormalAssaultScav(__state, ___botOwner_0))
                {
                    ___botOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetAssaultScavWildSpawnType(___botOwner_0, currentMapName);

                    return true; // Do original
                }

                if (AiHelpers.BotIsSptPmc(__state, ___botOwner_0))
                {
                    // Bot has inventory equipment
                    if (___botOwner_0.Profile?.Inventory?.Equipment != null)
                    {
                        pmcFoundInRaidEquipment.ConfigurePMCFindInRaidStatus(___botOwner_0);
                    }

                    ___botOwner_0.Profile.Info.Settings.Role = aIBrainSpawnWeightAdjustment.GetPmcWildSpawnType(___botOwner_0, ___botOwner_0.Profile.Info.Settings.Role, currentMapName);
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
        /// the client sometimes replaces PMC roles with 'assaultGroup', give PMCs their original role back (sptBear/sptUsec)
        /// </summary>
        /// <returns>WildSpawnType</returns>
        private static WildSpawnType FixAssaultGroupPmcsRole(BotOwner botOwner)
        {
            if (botOwner.Profile.Info.IsStreamerModeAvailable && botOwner.Profile.Info.Settings.Role == WildSpawnType.assaultGroup)
            {
                Logger.LogError($"Broken PMC found: {botOwner.Profile.Nickname}, was {botOwner.Profile.Info.Settings.Role}");

                // Its a PMC, figure out what the bot originally was and return it
                return botOwner.Profile.Info.Side == EPlayerSide.Bear
                    ? (WildSpawnType)AkiBotsPrePatcher.sptBearValue
                    : (WildSpawnType)AkiBotsPrePatcher.sptUsecValue;
            }

            // Not broken pmc, return original role
            return botOwner.Profile.Info.Settings.Role;
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
                // Set spt bot bot back to original type
                ___botOwner_0.Profile.Info.Settings.Role = __state;
            }
            else if (AiHelpers.BotIsPlayerScav(__state, ___botOwner_0))
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
