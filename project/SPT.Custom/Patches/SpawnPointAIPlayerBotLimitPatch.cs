using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace SPT.Custom.Patches
{
    /**
     * The purpose of this patch is to include AI PMCs in the individual limit checks for non-wave
     * spawn caps. This is to better mimic live PvP spawn behaviour, while not drastically increasing
     * the total spawn cap for each map
     */
    public class SpawnPointAIPlayerBotLimitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SpawnPoint), nameof(SpawnPoint.IsInPlayersIndividualLimits));
        }

        /**
         * Re-implement the original `IsInPlayersIndividualLimits` but including AI PMCs
         */
        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result, BotCreationDataClass creationData, Vector3 ___Position)
        {
            if (creationData == null)
            {
                __result = true;
                return false;
            }

            foreach (Profile profile in creationData.Profiles)
            {
                if ((profile.Info.Settings.Role.IsBossOrFollower() || profile.Info.Settings.Role == WildSpawnType.marksman) && profile.Info.Settings.Role != WildSpawnType.assaultGroup)
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
                    if (!player.IsAI || AiHelpers.BotIsSptPmc(player.Profile.Info.Settings.Role, player.AIData.BotOwner))
                    {
                        float dist = ___Position.SqrDistance(player.Position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closestPlayer = player;
                        }
                    }
                }

                if (closestPlayer != null)
                {
                    __result = botsController.BotSpawnLimiter.IsInPlayerSpawnLimit(closestPlayer, creationData);
                    return false;
                }
            }

            __result = false;

            // Skip original
            return false;
        }
    }
}
