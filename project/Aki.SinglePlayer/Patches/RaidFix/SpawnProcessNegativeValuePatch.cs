using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Reflection;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Prevent BotSpawnerClass from adjusting the spawn process value to be below 0
    /// This fixes aiamount = high spawning 80+ bots on maps like streets/customs
    /// int_0 = all bots alive
    /// int_1 = followers alive
    /// int_2 = bosses currently alive
    /// int_3 = spawn process? - current guess is open spawn positions - bsg doesnt seem to handle negative vaues well
    /// int_4 = max bots
    /// </summary>
    public class SpawnProcessNegativeValuePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.CheckOnMax));
        }

        [PatchPrefix]
        private static bool PatchPreFix(int wantSpawn, ref int toDelay, ref int toSpawn, ref int ____maxBots, int ____allBotsCount, int ____inSpawnProcess)
        {
            // Set bots to delay if alive bots + spawning bots count > maxbots
            // ____inSpawnProcess can be negative, don't go below 0 when calculating
            if ((____allBotsCount + Math.Max(____inSpawnProcess, 0)) > ____maxBots)
            {
                toDelay += wantSpawn;
                toSpawn = 0;

                return false; // Skip original
            }

            return true; // Do original
        }
    }
}