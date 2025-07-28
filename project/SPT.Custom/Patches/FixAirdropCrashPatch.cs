using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.SynchronizableObjects;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

/// <summary>
/// This patch prevents the crashing that was caused by the airdrop since the mortar event, it fully unloads whatever synchronized objects
/// Are still loaded before unused resources are cleaned up (Which causes this crash)
/// </summary>
public class FixAirdropCrashPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.FirstMethod(typeof(TarkovApplication), IsTargetMethod);
    }

    private static bool IsTargetMethod(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 4
               && parameters[0].Name == "profileId"
               && parameters[0].ParameterType == typeof(string)
               && parameters[1].Name == "savageProfile"
               && parameters[1].ParameterType == typeof(Profile)
               && parameters[2].Name == "location"
               && parameters[2].ParameterType == typeof(LocationSettingsClass.Location)
               && parameters[3].Name == "result"
               && parameters[3].ParameterType
               == typeof(Result<ExitStatus, TimeSpan, MetricsClass>);
    }

    [PatchPrefix]
    public static void Prefix()
    {
        if (!Singleton<GameWorld>.Instantiated)
        {
            return;
        }

        GameWorld gameWorld = Singleton<GameWorld>.Instance;
        if (gameWorld.SynchronizableObjectLogicProcessor is null)
        {
            return;
        }

        List<SynchronizableObject> syncObjects = gameWorld
            .SynchronizableObjectLogicProcessor
            .List_0;
        if (syncObjects is null)
        {
            return;
        }

        foreach (SynchronizableObject obj in syncObjects)
        {
            obj.Logic.ReturnToPool();
            obj.ReturnToPool();
        }

        // Without this check can cause black screen when backing out of raid prior to airdrop manager being init
        if (gameWorld.SynchronizableObjectLogicProcessor.AirdropManager is not null)
        {
            if (
                gameWorld.SynchronizableObjectLogicProcessor
                is SynchronizableObjectLogicProcessorClass synchronizableObjectLogicProcessorClass
            )
            {
                synchronizableObjectLogicProcessorClass.ServerAirdropManager?.Dispose();
            }

            gameWorld.SynchronizableObjectLogicProcessor.Dispose();
        }
    }
}