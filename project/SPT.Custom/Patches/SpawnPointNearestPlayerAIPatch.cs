using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Custom.Patches;

/// <summary>
/// Patch to include AI PMCs in the Nearest Player result list for the <see cref="SpawnPoint"/> class.
/// </summary>
/// <remarks>
/// This change enables spawn tracking for AI PMCs based on their unique ID,
/// rather than attributing all spawns to the main player.
/// </remarks>
public class SpawnPointNearestPlayerAIPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.FirstMethod(typeof(SpawnPoint), (method) => method.ReturnType == typeof(Player));
    }

    /// <summary>
    /// Re-implement the original method, but including AI PMCs
    /// </summary>
    /// <param name="__result"></param>
    /// <param name="___Position"></param>
    /// <returns></returns>
    [PatchPrefix]
    public static bool PatchPrefix(ref Player __result, Vector3 ___Position)
    {
        List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
        Player closestPlayer = null;
        float minDistance = float.MaxValue;
        if (Singleton<IBotGame>.Instantiated && Singleton<IBotGame>.Instance.BotsController != null)
        {
            foreach (Player player in allAlivePlayersList)
            {
                // Skip if this is an AI bot who isn't a PMC
                if (player.IsAI && !player.AIData.BotOwner.IsPMC())
                    continue;
                if (!player.HealthController.IsAlive)
                    continue;

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
