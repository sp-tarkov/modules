﻿using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.Progression
{
    public class LighthouseTransmitterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RadioTransmitterHandlerClass), nameof(RadioTransmitterHandlerClass.method_4));
        }

        [PatchPrefix]
        private static bool PatchPrefix(RadioTransmitterHandlerClass __instance)
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null) return false;

            var transmitter = __instance.RecodableComponent;

            if (transmitter.IsEncoded)
            {
                transmitter.SetStatus(RadioTransmitterStatus.Green);
            }
            else if (gameWorld.MainPlayer.IsAgressorInLighthouseTraderZone)
            {
                // this might need to be tested and changed as I don't think this currently is affect upon killing bosses
                transmitter.SetStatus(RadioTransmitterStatus.Yellow);
            }
            else
            {
                transmitter.SetStatus(RadioTransmitterStatus.Red);
            }

            return false;
        }
    }
}