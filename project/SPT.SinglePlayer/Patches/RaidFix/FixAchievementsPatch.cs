using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    public class FixAchievementsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Init));
        }

        [PatchPostfix]
        public static void PatchPostfix(Profile profile, InventoryControllerClass inventoryController, ref AbstractAchievementControllerClass ____achievementsController)
        {
            var achievementController = new GClass3233(profile, inventoryController, PatchConstants.BackEndSession, true);
            achievementController.Init();
            achievementController.Run();

            ____achievementsController = achievementController;
        }
    }
}