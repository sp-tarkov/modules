using System.Reflection;
using Aki.Reflection.Patching;

namespace Aki.SinglePlayer.Patches.Progression
{
    /// <summary>
    /// BSG have disabled notifications for local raids, set updateAchievements in the achievement controller to always be true
	/// This enables the achievement notifications and the client to save completed achievement data into profile.Achievements
    /// </summary>
    public class MidRaidAchievementChangePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(AchievementControllerClass).GetConstructors()[0];
		}

		[PatchPrefix]
		private static bool PatchPrefix(ref bool updateAchievements)
		{
			updateAchievements = true;

			return true; // Do original method
		}
	}
}