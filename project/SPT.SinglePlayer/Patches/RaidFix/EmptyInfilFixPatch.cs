using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// An empty EntryPoint string (string_0 in BaseLocalGame) causes exfil point initialization to be skipped.
/// This patch sets an EntryPoint string if it's missing.
/// </summary>
public class EmptyInfilFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        var desiredType = PatchConstants.LocalGameType.BaseType;
        var desiredMethod = desiredType
            .GetMethods(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.DeclaredOnly
                | BindingFlags.CreateInstance
            )
            .SingleCustom(IsTargetMethod);

        Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
        Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

        return desiredMethod;
    }

    private static bool IsTargetMethod(MethodInfo methodInfo)
    {
        return (
            methodInfo.IsVirtual
            && methodInfo.GetParameters().Length == 0
            && methodInfo.ReturnType == typeof(void)
            && methodInfo.GetMethodBody().LocalVariables.Count > 0
        );
    }

    [PatchPrefix]
    public static void PatchPrefix(ref string ___string_0)
    {
        if (!string.IsNullOrWhiteSpace(___string_0))
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

        ___string_0 = closestSpawn.SpawnPoint.Infiltration;
    }
}