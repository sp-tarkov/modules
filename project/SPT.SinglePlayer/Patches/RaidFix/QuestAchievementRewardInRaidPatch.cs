using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /**
     * The purpose of this patch is to allow quests in-raid to reward achievements, and have them actually trigger
     */
    public class QuestAchievementRewardInRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Init));
        }

        [PatchPostfix]
        private static async void PatchPostfix(Task __result, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController, bool aiControlled)
        {
            // Wait for original to finish
            await __result;

            // If this isn't an AI, properly setup the achievement unlocked handler
            if (!aiControlled && questController != null && achievementsController != null)
            {
                // This should be called rarely enough that the memory overhead isn't a concern
                questController.AchievementUnlocked += (achId) => achievementsController.UnlockAchievementForced(achId);
            }
        }
    }
}
