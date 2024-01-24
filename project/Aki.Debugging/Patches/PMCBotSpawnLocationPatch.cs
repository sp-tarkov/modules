using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using EFT.UI;
using HarmonyLib;
using System.Collections.Generic;
using EFT.Game.Spawning;
using System;
using Aki.PrePatch;

namespace Aki.Debugging.Patches
{

    // TODO: Instantiation of this is fairly slow, need to find best way to cache it
    public class SptSpawnHelper
    {
        private readonly List<ISpawnPoint> playerSpawnPoints;
        private readonly Random _rnd = new Random();
        private readonly GStruct379 _spawnSettings = new GStruct379();

        public SptSpawnHelper()
        {
            IEnumerable<ISpawnPoint> locationSpawnPoints = GClass2924.CreateFromScene();

            var playerSpawns = locationSpawnPoints.Where(x => x.Categories.HasFlag(ESpawnCategoryMask.Player)).ToList();
            this.playerSpawnPoints = locationSpawnPoints.Where(x => x.Categories.HasFlag(ESpawnCategoryMask.Player)).ToList();
        }

        public void PrintSpawnPoints()
        {
            foreach (var spawnPoint in playerSpawnPoints)
            {
                ConsoleScreen.Log("[AKI PMC Bot spawn] Spawn point " + spawnPoint.Id + " location is " + spawnPoint.Position.ToString());
            }
        }

        public ISpawnPoint SelectSpawnPoint()
        {
            // TODO: Select spawn points more intelligently
            return this.playerSpawnPoints[_rnd.Next(this.playerSpawnPoints.Count)];
        }

        public List<ISpawnPoint> SelectSpawnPoints(int count)
        {
            // TODO: Fine-grained spawn selection
            if (count > this.playerSpawnPoints.Count())
            {
                ConsoleScreen.Log($"[AKI PMC Bot spawn] Wanted ${count} but only {this.playerSpawnPoints.Count()} found, returning all");
                return this.playerSpawnPoints;
            }
            return this.playerSpawnPoints.OrderBy(x => _rnd.Next()).Take(count).ToList();
        }
    }


    public class PMCBotSpawnLocationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), "TryToSpawnInZoneInner");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass1470 __instance, GClass588 data)
        {

            var firstBotRole = data.Profiles[0].Info.Settings.Role;
            if ((int)firstBotRole != AkiBotsPrePatcher.sptBearValue || (int)firstBotRole != AkiBotsPrePatcher.sptUsecValue)
            {
                ConsoleScreen.Log("[AKI PMC Bot spawn] Spawning a set of Scavs. Skipping...");
                return true;
            }

            var helper = new SptSpawnHelper();
            var newSpawns = helper.SelectSpawnPoints(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                ConsoleScreen.Log($"[AKI PMC Bot spawn] Trying to spawn bot {i}");
                var currentSpawnData = data.Separate(1);

                // Unset group settings
                // TODO: Allow for PMC bot groups?
                currentSpawnData.SpawnParams.ShallBeGroup = null;
                var spawnPointDetails = newSpawns[i];
                var currentZone = __instance.GetClosestZone(spawnPointDetails.Position, out float _);

                // CorePointId of player spawns seems to always be 0. Bots will not activate properly if this ID is used
                // TODO: Verify if CorePointId of 1 is acceptable in all cases
                
                ConsoleScreen.Log($"[AKI PMC Bot spawn] spawn point chosen: {spawnPointDetails.Name} Core point id was: {spawnPointDetails.CorePointId}");
                currentSpawnData.AddPosition(spawnPointDetails.Position, spawnPointDetails.CorePointId);

                __instance.SpawnBotsInZoneOnPositions(newSpawns.GetRange(i, 1), currentZone, currentSpawnData);
            }

            return false;
        }
    }
}