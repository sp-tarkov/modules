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
		public void PostFixPatch()
		{
			try
			{
				var gameworld = Singleton<GameWorld>.Instance;
				var botGame = Singleton<IBotGame>.Instance;

				gameworld.BtrController = new GClass2911();
				botGame.BotsController.BotSpawner.SpawnBotBTR();

				var btrTransform = gameworld.BtrController?.BtrVehicle?.gameObject?.transform;

				if (btrTransform != null)
				{
					ConsoleScreen.Log($"[AKI-BTR] Btr Location {btrTransform}");
				} else {
					ConsoleScreen.Log($"[AKI-BTR] wasnt able to get BTR location");
				}
			}
			catch (System.Exception)
			{
				ConsoleScreen.Log("[AKI-BTR] Exception thrown, check logs");
				throw;
			}
		}
	}
}