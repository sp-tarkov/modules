using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.ScavMode;

public class ScavRepAdjustmentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // Correct Gclass has sessionCounters
        return AccessTools.Method(
            typeof(BaseStatisticsManager),
            nameof(BaseStatisticsManager.OnEnemyKill)
        );
    }

    [PatchPrefix]
    public static void PatchPrefix(DamageInfo damage, string playerProfileId)
    {
        var player = (Player)damage.Player.iPlayer;

        // Add safeguards to make sure no calculations happen from other bots
        if (!player.IsYourPlayer)
        {
            Logger.LogError("This shouldn't be happening. Are you sure we are using the correct GClass?");
            return;
        }

        if (player.Profile.Side != EPlayerSide.Savage)
        {
            return;
        }

        if (Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(playerProfileId) is Player killedBot)
        {
            var killedPlayerSettings = killedBot.Profile.Info.Settings;

            // If Victim is a PMC and has killed a Scav or Marksman.
            if (killedPlayerSettings.Role is WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC)
            {
                if (HasBotKilledScav(killedBot))
                {
                    player.Profile.FenceInfo.AddStanding(killedPlayerSettings.StandingForKill, EFT.Counters.EFenceStandingSource.ScavHelp);
                }
            }
            else
            {
                player.Loyalty.GifterKill(killedBot);
            }
        }
    }

    private static bool HasBotKilledScav(Player killedPlayer)
    {
        var killedBots = killedPlayer.Profile.EftStats.Victims;

        foreach (var Bot in killedBots)
        {
            if (Bot.Role == WildSpawnType.assault || Bot.Role == WildSpawnType.marksman || Bot.Role == WildSpawnType.assaultGroup)
            {
                return true;
            }
        }

        return false;
    }
}
