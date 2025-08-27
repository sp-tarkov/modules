
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SPT.Custom.Patches
{
    /**
     * The purpose of this patch is to include AI PMCs in the Nearest Player result list for 
     * the SpawnPoint class. This allows AI PMCs to have spawns tracked based on their ID instead
     * of all spawns being tracked to the main player
     */
    public class SpawnPointNearestPlayerAIPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(SpawnPoint), (method) => method.ReturnType == typeof(Player));
        }

        /**
         * Re-implement the original method, but including AI PMCs
         */
        [PatchPrefix]
        public static bool PatchPrefix(ref Player __result, Vector3 ___Position)
        {
            List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
            Player closestPlayer = null;
            float minDistance = float.MaxValue;
            if (Singleton<IBotGame>.Instance?.BotsController != null)
            {
                foreach (Player player in allAlivePlayersList)
                {
                    // Skip if this is an AI bot who isn't a PMC
                    if (player.IsAI && !AiHelpers.BotIsSptPmc(player.Profile.Info.Settings.Role, player.AIData.BotOwner)) continue;
                    if (!player.HealthController.IsAlive) continue;

                    float dist = ___Position.SqrDistance(((IPlayer)player).Position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestPlayer = player;
                    }
                }
            }

            __result = closestPlayer;

            // Skip original
            return false;
        }
    }
}
