using SPT.Common.Http;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Adds ability for some scavs to be hostile to player scavs
    /// Allow player to kill these hostile scavs with no repercussions
    /// Gets config data from server
    /// </summary>
    public class AddTraitorScavsPatch : ModulePatch
    {
        private static int? TraitorChancePercent;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.GetGroupAndSetEnemies));
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotsGroup __result, IBotGame ____game, DeadBodiesController ____deadBodiesController, BotOwner bot, BotZone zone)
        {
            if (!TraitorChancePercent.HasValue)
            {
                string json = RequestHandler.GetJson("/singleplayer/scav/traitorscavhostile");
                TraitorChancePercent = JsonConvert.DeserializeObject<int>(json);
            }

            WildSpawnType role = bot.Profile.Info.Settings.Role;
            if (AiHelpers.BotIsPlayerScav(role, bot.Profile.Info.Nickname) && new Random().Next(1, 100) < TraitorChancePercent)
            {
                Logger.LogInfo($"Making {bot.name} ({bot.Profile.Nickname}) hostile to player");

                // Create a new group for this scav itself to belong to
                var player = Singleton<GameWorld>.Instance.MainPlayer;
                var enemies = new List<BotOwner>();
                var players = new List<Player>() { player };
                var botsGroup = new BotsGroup(zone, ____game, bot, enemies, ____deadBodiesController, players, false);

                // Because we don't want to use the zone-specific group, we add the new group with no key. This is similar to free for all
                Singleton<IBotGame>.Instance.BotsController.BotSpawner.Groups.AddNoKey(botsGroup, zone);
                botsGroup.AddEnemy(player, EBotEnemyCause.checkAddTODO);

                // Make it so the player can kill the scav without aggroing the rest of the scavs
                bot.Loyalty.CanBeFreeKilled = true;

                // Traitors dont talk
                bot.BotTalk.SetSilence(9999);

                // Return our new botgroup
                __result = botsGroup;

                // Skip original
                return false;
            }

            return true; // Do original method
        }
    }
}