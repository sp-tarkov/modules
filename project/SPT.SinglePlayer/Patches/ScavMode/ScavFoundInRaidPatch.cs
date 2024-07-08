using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class ScavFoundInRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        private static void PatchPrefix(GameWorld __instance)
        {
            var player = __instance.MainPlayer;

            if (player == null || player.Profile.Side != EPlayerSide.Savage)
            {
                return;
            }

            foreach (var item in player.Profile.Inventory.AllRealPlayerItems)
            {
                item.SpawnedInSession = true;
            }
        }
    }
}
