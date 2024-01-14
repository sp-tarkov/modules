﻿using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    // The BTRManager MapPathsConfiguration loading depends on the game state being set to Starting
    // so set it to Starting while the method is running, then reset it afterwards
    public class BTRPathLoadPatch : ModulePatch
    {
        private static PropertyInfo _statusProperty;
        private static GameStatus originalStatus;
        protected override MethodBase GetTargetMethod()
        {
            _statusProperty = AccessTools.Property(typeof(AbstractGame), nameof(AbstractGame.Status));

            return AccessTools.Method(typeof(BTRControllerClass), "method_1");
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            originalStatus = Singleton<AbstractGame>.Instance.Status;
            _statusProperty.SetValue(Singleton<AbstractGame>.Instance, GameStatus.Starting);
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            _statusProperty.SetValue(Singleton<AbstractGame>.Instance, originalStatus);
        }
    }
}
