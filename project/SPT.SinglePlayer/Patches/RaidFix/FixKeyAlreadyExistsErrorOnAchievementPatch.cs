using System.Reflection;
using EFT.Achievements;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

public class FixKeyAlreadyExistsErrorOnAchievementPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(AchievementsController),
            nameof(AchievementsController.OnConditionalStatusChangedEvent)
        );
    }

    [PatchPrefix]
    public static bool Prefix(Achievement achievement, bool notify, AchievementsController __instance)
    {
        if (achievement.IsDone() && __instance.Profile.AchievementsData.ContainsKey(achievement.Id))
        {
            // Tries to add same achievement key a second time, throwing a "An item with the same key has already been added" error
            return false; // Skip original
        }

        return true; // Do original
    }
}
