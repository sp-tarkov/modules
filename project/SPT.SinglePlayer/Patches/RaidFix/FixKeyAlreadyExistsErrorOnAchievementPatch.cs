using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    public class FixKeyAlreadyExistsErrorOnAchievementPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AbstractAchievementControllerClass), nameof(AbstractAchievementControllerClass.OnConditionalStatusChangedEvent));
        }

        [PatchPrefix]
        public static bool Prefix(AchievementTaskClass achievement, bool notify, AbstractAchievementControllerClass __instance)
        {
            if (achievement.IsDone() && __instance.Profile.AchievementsData.ContainsKey(achievement.Id))
            {
                // Tries to add same achievement key a second time, throwing a "An item with the same key has already been added" error
                return false; // Skip original
            }

            return true; // Do original
        }
    }
}
