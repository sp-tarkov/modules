using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class ScavRepAdjustmentPatch : ModulePatch
    {
        // TODO: REMAP/UPDATE GCLASS REF
        protected override MethodBase GetTargetMethod()
        {
            // Correct Gclass has sessionCounters
            return AccessTools.Method(typeof(GClass1801), nameof(GClass1801.OnEnemyKill));
        }

        [PatchPrefix]
        private static void PatchPrefix(DamageInfo damage, string playerProfileId, out Tuple<Player, bool> __state)
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

            if (Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(playerProfileId) is Player killedPlayer)
            {
                __state = new Tuple<Player, bool>(killedPlayer, killedPlayer.AIData.IsAI);
                // Extra check to ensure we only set playerscavs to IsAI = false
                if (killedPlayer.Profile.Info.Settings.Role == WildSpawnType.assault && killedPlayer.Profile.Nickname.Contains("("))
                {
                    killedPlayer.AIData.IsAI = false;
                }

                player.Loyalty.method_1(killedPlayer);
            }
        }
        [PatchPostfix]
        private static void PatchPostfix(Tuple<Player, bool> __state)
        {
            if(__state.Item1 != null)
            {
                __state.Item1.AIData.IsAI = __state.Item2;
            }
        }
    }
}
