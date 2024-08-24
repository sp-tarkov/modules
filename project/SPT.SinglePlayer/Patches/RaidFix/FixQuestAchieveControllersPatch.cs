using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// this patch aims to allow achievements and quests to activate/change and finish whilst inraid
    /// </summary>
    public class FixQuestAchieveControllersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Init));
        }

        [PatchPostfix]
        public static void PatchPostfix(Profile profile, InventoryController inventoryController, ref AbstractQuestControllerClass ____questController, ref AbstractAchievementControllerClass ____achievementsController)
        {
            var questController = new LocalQuestControllerClass(profile, inventoryController, PatchConstants.BackEndSession, true);
            questController.Init();
            questController.Run();

            var achievementController =
                new AchievementControllerClass(profile, inventoryController, PatchConstants.BackEndSession, true);
            achievementController.Init();
            achievementController.Run();

            ____questController = questController;
            ____achievementsController = achievementController;
        }
    }
}