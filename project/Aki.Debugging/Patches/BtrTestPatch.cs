using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI;

namespace Aki.Debugging.Patches
{
	public class BtrTestPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
		}

        [PatchPrefix]
        public static void PatchPrefix()
		{
			try
			{
                var gameWorld = Singleton<GameWorld>.Instance;
				if (gameWorld.MainPlayer.Location.ToLower() != "tarkovstreets")
				{
					// only run patch on streets
					return;
				}

                if (gameWorld.BtrController == null)
				{
                    ConsoleScreen.LogWarning($"[AKI-BTR] pre GClass2911 instance is null: {Singleton<GClass2911>.Instance == null}");
                    gameWorld.BtrController = Singleton<GClass2911>.Instance;

                    ConsoleScreen.LogWarning($"[AKI-BTR] pre BtrController instance is null: {gameWorld.BtrController == null}");
                    ConsoleScreen.LogWarning($"[AKI-BTR] pre BtrController.BotShooterBtr instance is null: {gameWorld.BtrController?.BotShooterBtr == null}");
                }
            }
			catch (System.Exception)
			{
                ConsoleScreen.LogError("[AKI-BTR] Prepatch Exception thrown, check logs");
                throw;
            }
		}

        [PatchPostfix]
		public static void PatchPostfix()
		{
			try
			{
				var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld.MainPlayer.Location.ToLower() != "tarkovstreets")
                {
                    // only run patch on streets
                    return;
                }

                var botGame = Singleton<IBotGame>.Instance;

                ConsoleScreen.LogWarning("[AKI-BTR] Post patch, spawning btr");

                ConsoleScreen.LogWarning($"[AKI-BTR] botspawner is enabled: {botGame.BotsController.IsEnable}");
                botGame.BotsController.BotSpawner.SpawnBotBTR();

                ConsoleScreen.LogWarning($"[AKI-BTR] btr vehicle is null: {gameWorld.BtrController?.BtrVehicle == null}");
                ConsoleScreen.LogWarning($"[AKI-BTR] btr vehicle gameobject is null: {gameWorld.BtrController?.BtrVehicle?.gameObject == null}");
                ConsoleScreen.LogWarning($"[AKI-BTR] BtrController.BotShooterBtr instance is null: {gameWorld.BtrController?.BotShooterBtr == null}");

                var btrTransform = gameWorld.BtrController?.BtrVehicle?.gameObject?.transform;

                if (btrTransform != null)
				{
					ConsoleScreen.LogWarning($"[AKI-BTR] Btr Location {btrTransform}");
				} else {
					ConsoleScreen.LogWarning($"[AKI-BTR] wasnt able to get BTR location");
				}
			}
			catch (System.Exception)
			{
				ConsoleScreen.LogError("[AKI-BTR] Exception thrown, check logs");
				throw;
			}
		}
	}
}