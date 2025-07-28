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
/// Patch that includes AI PMCs in the individual limit checks for non-wave spawn caps <br/>
/// This change helps mimic live PvP spawn behavior more accurately <br/>
/// without significantly increasing the total spawn cap for each map
/// </summary>
public class SpawnPointAIPlayerBotLimitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(SpawnPoint),
            nameof(SpawnPoint.IsInPlayersIndividualLimits)
        );
    }

    /// <summary>
    /// Re-implement the original <see cref="SpawnPoint.IsInPlayersIndividualLimits"/> but including AI PMCs
    /// </summary>
    /// <param name="__result"></param>
    /// <param name="creationData"></param>
    /// <param name="___Position"></param>
    /// <returns></returns>
    [PatchPrefix]
    public static bool PatchPrefix(
        ref bool __result,
        BotCreationDataClass creationData,
        Vector3 ___Position
    )
    {
        if (creationData == null)
        {
            __result = true;
            return false;
        }

        foreach (Profile profile in creationData.Profiles)
        {
            if (
                (
                    profile.Info.Settings.Role.IsBossOrFollower()
                    || profile.Info.Settings.Role == WildSpawnType.marksman
                )
                && profile.Info.Settings.Role != WildSpawnType.assaultGroup
            )
            {
                __result = true;
                return false;
            }
        }

        BotsController botsController = Singleton<IBotGame>.Instance?.BotsController;
        if (botsController != null)
        {
            float minDistance = float.MaxValue;
            Player closestPlayer = null;
            foreach (Player player in Singleton<GameWorld>.Instance.AllAlivePlayersList)
            {
                if (!player.IsAI || player.AIData.BotOwner.IsPMC())
                {
                    float dist = ___Position.SqrDistance(((IPlayer)player).Position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestPlayer = player;
                    }
                }
            }

            if (closestPlayer != null)
            {
                __result = botsController.BotSpawnLimiter.IsInPlayerSpawnLimit(
                    closestPlayer,
                    creationData
                );
                return false;
            }
        }

        __result = false;

        // Skip original
        return false;
    }
}