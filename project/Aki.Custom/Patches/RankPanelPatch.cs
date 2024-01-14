﻿using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;
using System.Reflection;
using HarmonyLib;

namespace Aki.Custom.Patches
{
    public class RankPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RankPanel), nameof(RankPanel.Show));
        }

        [PatchPrefix]
        private static bool PatchPreFix(ref int rankLevel, ref int maxRank)
        {
            if (Singleton<GameWorld>.Instance != null)
            {
                Logger.LogWarning("Rank Level: " + rankLevel.ToString() + " Max Rank Level: " + maxRank.ToString());
                ConsoleScreen.LogError("Rank Level: " + rankLevel.ToString() + " Max Rank Level: " + maxRank.ToString());
                ConsoleScreen.LogError("Game Broke!");
                Logger.LogWarning("This Shouldn't happen!! Please report this in discord");
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
            }
            return true;
        }
    }
}