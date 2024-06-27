﻿using SPT.Common.Http;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace SPT.Custom.Patches
{   
    /// <summary>
    /// Fixes the map sandbox from only spawning 1 bot at start of game as well as fixing no spawns till all bots are dead.
    /// Remove once BSG decides to fix their map
    /// </summary>
    public class FixBrokenSpawnOnSandboxPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                return;
            }

			var playerLocation = gameWorld.MainPlayer.Location;

            if (playerLocation == "Sandbox" || playerLocation == "Sandbox_high")
            {
				LocationScene.GetAll<BotZone>().ToList().First(zone => zone.name == "ZoneSandbox").MaxPersonsOnPatrol = GetMaxPatrolValueFromServer();
            }
		}

		public static int GetMaxPatrolValueFromServer()
		{
			string json = RequestHandler.GetJson("/singleplayer/sandbox/maxpatrol");
			return JsonConvert.DeserializeObject<int>(json);
		}
	}
}
