using System.Reflection;
using Aki.Reflection.Patching;
using AchievementsController = GClass3207;

namespace Aki.SinglePlayer.Patches.Progression
{
	public class MidRaidAchievementChangePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(AchievementsController).GetConstructors()[0];
		}

		[PatchPrefix]
		private static bool PatchPrefix(ref bool updateAchievements)
		{
			updateAchievements = true;
			return true;
		}
	}
}