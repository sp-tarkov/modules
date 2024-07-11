using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Aim of this patch is just to apply the backend session to the controller and supply true to updating achievements,
    /// the mappings for these also need fixing. 3.8.3 remaps a different class compared to 3.9.0 and 3.10.0.
    /// </summary>
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