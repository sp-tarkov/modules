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
        private static int? _traitorChancePercent;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.GetGroupAndSetEnemies));
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotsGroup __result, BotOwner bot, BotZone zone, BotSpawner __instance)
        {
            if (!_traitorChancePercent.HasValue)
            {
                string json = RequestHandler.GetJson("/singleplayer/scav/traitorscavhostile");
                _traitorChancePercent = JsonConvert.DeserializeObject<int>(json);
            }

            if (_traitorChancePercent == 0)
            {
                return true; // Do original method
            }

            WildSpawnType role = bot.Profile.Info.Settings.Role;
            if (AiHelpers.BotIsSimulatedPlayerScav(role, bot.Profile.Info.MainProfileNickname) && new Random().Next(1, 100) < _traitorChancePercent)
            {
                Logger.LogInfo($"Making {bot.name} ({bot.Profile.Nickname}) hostile to player");

                // Create a new group for this scav itself to belong to
                var player = Singleton<GameWorld>.Instance.MainPlayer;
                var enemies = new List<BotOwner>();
                var players = new List<Player>() { player };
                var botsGroup = new BotsGroup(zone, __instance.BotGame, bot, enemies, __instance.DeadBodiesController, players, false);

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