﻿using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class AmmoUsedCounterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnMakingShot));
        }

        [PatchPostfix]
        public static void PatchPostfix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                __instance.Profile.EftStats.SessionCounters.AddLong(1L, SessionCounterTypesAbstractClass.AmmoUsed);
            }
        }
    }
}
