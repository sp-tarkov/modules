using Aki.Reflection.Patching;
using EFT.HealthSystem;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using Comfort.Common;
using EFT.UI;
using EFT.Ballistics;
using static EFT.Player.FirearmController;


namespace Aki.Debugging.Patches.Stats
{
    public class AmmoUsedPatch : ModulePatch
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
            player.Profile.EftStats.SessionCounters.AddLong(1L, GClass2200.AmmoUsed);
        }
    }
}
