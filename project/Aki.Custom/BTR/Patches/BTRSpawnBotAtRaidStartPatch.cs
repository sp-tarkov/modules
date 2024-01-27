using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Aki.Custom.BTR.Patches
{
    public class BTRSpawnBotAtRaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            var btrManager = __instance.GetComponent<BTRManager>();
            if (btrManager == null)
            {
                return;
            }

            var botGame = Singleton<IBotGame>.Instance;
            if (botGame == null)
            {
                return;
            }

            botGame.BotsController.BotSpawner.SpawnBotBTR();
        }
    }
}
