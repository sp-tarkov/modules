using EFT.SynchronizableObjects;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// This patch prevents the crashing that was caused by the airdrop since the mortar event, it fully unloads whatever synchronized objects
    /// Are still loaded before unused resources are cleaned up (Which causes this crash)
    /// </summary>
    public class FixAirdropCrashPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_48));
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

            List<SynchronizableObject> syncObjects = Traverse.Create(gameWorld.SynchronizableObjectLogicProcessor)?.Field<List<SynchronizableObject>>("list_0")?.Value;
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
                if (gameWorld.SynchronizableObjectLogicProcessor is SynchronizableObjectLogicProcessorClass synchronizableObjectLogicProcessorClass)
                {
                    synchronizableObjectLogicProcessorClass.ServerAirdropManager?.Dispose();
                }

                gameWorld.SynchronizableObjectLogicProcessor.Dispose();
            }
        }
    }
}
