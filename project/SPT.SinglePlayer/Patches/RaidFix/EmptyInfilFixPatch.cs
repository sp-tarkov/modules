using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// An empty EntryPoint string (string_0 in BaseLocalGame) causes exfil point initialization to be skipped.
/// This patch sets an EntryPoint string if it's missing.
/// Leaving the above as history:
/// BaseLocalGame now includes _entryPoint and is used in the method we target (vmethod_6)
/// </summary>
public class EmptyInfilFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseLocalGame<EftGamePlayerOwner>), nameof(BaseLocalGame<EftGamePlayerOwner>.vmethod_6));
    }

    [PatchPrefix]
    public static void PatchPrefix(ref string ____entryPoint)
    {
        if (!string.IsNullOrWhiteSpace(____entryPoint))
        {
            return;
        }

        var spawnPoints = Resources.FindObjectsOfTypeAll<SpawnPointMarker>().ToList();

        List<SpawnPointMarker> filtered = new List<SpawnPointMarker>();

        foreach (var spawn in spawnPoints)
        {
            if (!string.IsNullOrEmpty(spawn?.SpawnPoint?.Infiltration?.Trim()))
            {
                filtered.Add(spawn);
            }
        }

        var playerPos = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
        SpawnPointMarker closestSpawn = null;
        var minDist = Mathf.Infinity;

        foreach (var filter in filtered)
        {
            var dist = Vector3.Distance(filter.gameObject.transform.position, playerPos);

            if (dist < minDist)
            {
                closestSpawn = filter;
                minDist = dist;
            }
        }

        ____entryPoint = closestSpawn.SpawnPoint.Infiltration;
    }
}
