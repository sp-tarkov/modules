using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using Comfort.Common;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    public class AmmoUsedCounterPatch : ModulePatch
    {
        private static Player player;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
            var firearmsController = player.HandsController as Player.FirearmController;
            firearmsController.OnShot += Hook;
        }

        private static void Hook()
        {
            player.Profile.EftStats.SessionCounters.AddLong(1L, SessionCounterTypesAbstractClass.AmmoUsed);
        }
    }
}
