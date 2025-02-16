using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using EFT.UI;
using HarmonyLib;
using System.Collections.Generic;
using EFT.Game.Spawning;
using System;

namespace SPT.Debugging.Patches
{

    // TODO: Instantiation of this is fairly slow, need to find best way to cache it
    public class SptSpawnHelper
    {
        private readonly List<ISpawnPoint> _playerSpawnPoints;
        private readonly Random _rnd = new Random();
        //private readonly GStruct381 _spawnSettings = new GStruct381();

        public SptSpawnHelper()
        {
            IEnumerable<ISpawnPoint> locationSpawnPoints = SpawnPointManagerClass.CreateFromScene();

            var playerSpawns = locationSpawnPoints.Where(x => x.Categories.HasFlag(ESpawnCategoryMask.Player)).ToList();
            this._playerSpawnPoints = locationSpawnPoints.Where(x => x.Categories.HasFlag(ESpawnCategoryMask.Player)).ToList();
        }

        public void PrintSpawnPoints()
        {
            foreach (var spawnPoint in _playerSpawnPoints)
            {
                ConsoleScreen.Log("[SPT PMC Bot spawn] Spawn point " + spawnPoint.Id + " location is " + spawnPoint.Position.ToString());
            }
        }

        public ISpawnPoint SelectSpawnPoint()
        {
            // TODO: Select spawn points more intelligently
            return this._playerSpawnPoints[_rnd.Next(this._playerSpawnPoints.Count)];
        }

        public List<ISpawnPoint> SelectSpawnPoints(int count)
        {
            // TODO: Fine-grained spawn selection
            if (count > this._playerSpawnPoints.Count)
            {
                ConsoleScreen.Log($"[SPT PMC Bot spawn] Wanted: ${count} but only {this._playerSpawnPoints.Count()} spawn points found, returning all");
                return this._playerSpawnPoints;
            }
            return this._playerSpawnPoints.OrderBy(x => _rnd.Next()).Take(count).ToList();
        }
    }


    public class PMCBotSpawnLocationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.TryToSpawnInZoneInner));
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotSpawner __instance, BotCreationDataClass data)
        {
            var firstBotRole = data.Profiles[0].Info.Settings.Role;
            if (firstBotRole is not (WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC))
            {
                ConsoleScreen.Log("[SPT PMC Bot spawn] Spawning a set of Scavs. Skipping...");
                return true; // Do original method
            }

            var helper = new SptSpawnHelper();
            var newSpawns = helper.SelectSpawnPoints(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                ConsoleScreen.Log($"[SPT PMC Bot spawn] Trying to spawn bot {i}");
                var currentSpawnData = data.Separate(1);

                // Unset group settings
                // TODO: Allow for PMC bot groups?
                currentSpawnData.SpawnParams.ShallBeGroup = null;
                var spawnPointDetails = newSpawns[i];
                var currentZone = __instance.GetClosestZone(spawnPointDetails.Position, out float _);

                // CorePointId of player spawns seems to always be 0. Bots will not activate properly if this ID is used
                // TODO: Verify if CorePointId of 1 is acceptable in all cases
                
                ConsoleScreen.Log($"[SPT PMC Bot spawn] spawn point chosen: {spawnPointDetails.Name} Core point id was: {spawnPointDetails.CorePointId}");
                currentSpawnData.AddPosition(spawnPointDetails.Position, spawnPointDetails.CorePointId);

                __instance.SpawnBotsInZoneOnPositions(newSpawns.GetRange(i, 1), currentZone, currentSpawnData);
            }

            return false; // Skip original
        }
    }
}