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
            typeof(GClass2267),
            nameof(GClass2267.OnEnemyKill)
        );
    }

    [PatchPrefix]
    public static void PatchPrefix(DamageInfo damage, string playerProfileId, out Tuple<Player, bool> __state)
    {
        __state = new Tuple<Player, bool>(null, false);
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
            __state = new Tuple<Player, bool>(killedBot, killedBot.AIData.IsAI);
            var killedPlayerSettings = killedBot.Profile.Info.Settings;
            // Extra check to ensure we only set playerscavs to IsAI = false
            if (killedPlayerSettings.Role == WildSpawnType.assault && killedBot.Profile.Nickname.Contains("("))
            {
                //killedBot.AIData.IsAI = false;
            }

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
                player.Loyalty.method_1(killedBot);
            }
        }
    }

    [PatchPostfix]
    private static void PatchPostfix(Tuple<Player, bool> __state)
    {
        if (__state.Item1 != null)
        {
            //__state.Item1.AIData.IsAI = __state.Item2;
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
