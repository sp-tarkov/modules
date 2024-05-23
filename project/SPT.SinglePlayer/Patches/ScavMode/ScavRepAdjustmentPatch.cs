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
            return AccessTools.Method(typeof(GClass1798), nameof(GClass1798.OnEnemyKill));
        }

        [PatchPrefix]
        private static void PatchPrefix(string playerProfileId, out Tuple<Player, bool> __state)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            __state = new Tuple<Player, bool>(null, false);

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
