﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Aki.SinglePlayer.Utils.MainMenu;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Prevents loading of non-whitelisted client mods to minimize the amount of false issue reports being made during the public BE phase
    /// </summary>
    public class PreventClientModsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.method_20));
        }

        [PatchPrefix]
        private static void Prefix()
        {
            CheckForNonWhitelistedPlugins(Logger);
        }

        private static void CheckForNonWhitelistedPlugins(ManualLogSource logger)
        {
            if (MenuNotificationManager.disallowedPlugins.Any())
            {
                logger.LogError($"{MenuNotificationManager.release.illegalPluginsLoadedText}\n{string.Join("\n", MenuNotificationManager.disallowedPlugins)}");
                throw new Exception(MenuNotificationManager.release.illegalPluginsExceptionText);
            }
        }
    }
}