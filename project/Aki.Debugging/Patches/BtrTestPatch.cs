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

                if (gameWorld.BtrController == null)
                {
                    if (!Singleton<GClass2911>.Instantiated)
                    {
                        Singleton<GClass2911>.Create(new GClass2911());
                    }

                    gameWorld.BtrController = Singleton<GClass2911>.Instance;

                    ConsoleScreen.LogWarning($"[AKI-BTR] BtrController instance is null: {gameWorld.BtrController == null}");
                    ConsoleScreen.LogWarning($"[AKI-BTR] Singleton GClass2911 is null: {Singleton<GClass2911>.Instance == null}");
                    ConsoleScreen.LogWarning($"[AKI-BTR] BtrController.BotShooterBtr instance is null: {gameWorld.BtrController?.BotShooterBtr == null}");
                    ConsoleScreen.LogWarning($"[AKI-BTR] BtrController.BtrVehicle instance is null: {gameWorld.BtrController?.BtrVehicle == null}");
                }
                ConsoleScreen.LogWarning($"[AKI-BTR] botspawner is enabled: {botGame.BotsController.IsEnable}");

                ConsoleScreen.LogWarning("[AKI-BTR] Post patch, spawning btr");
                botGame.BotsController.BotSpawner.SpawnBotBTR();

                ConsoleScreen.LogWarning($"[AKI-BTR] btr vehicle is null: {gameWorld.BtrController?.BtrVehicle == null}");
                ConsoleScreen.LogWarning($"[AKI-BTR] btr vehicle gameobject is null: {gameWorld.BtrController?.BtrVehicle?.gameObject == null}");
                ConsoleScreen.LogWarning($"[AKI-BTR] BtrController.BotShooterBtr instance is null: {gameWorld.BtrController?.BotShooterBtr == null}");

                var btrTransform = gameWorld.BtrController?.BtrVehicle?.gameObject?.transform;
                if (btrTransform != null)
				{
					ConsoleScreen.LogWarning($"[AKI-BTR] Btr Location {btrTransform}");
				} else
                {
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