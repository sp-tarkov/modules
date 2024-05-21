using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.Progression
{
    /**
     * There is no reason to update quest counters when exiting the hideout, so set the
     * player's QuestController to null while calling HideoutPlayer.OnGameSessionEnd to
     * avoid the quest controller counters from being triggered
     * 
     * Note: Player.OnGameSessionEnd handles the player's quest controller not being set gracefully
     */
    public class HideoutQuestIgnorePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutPlayer), nameof(HideoutPlayer.OnGameSessionEnd));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref AbstractQuestControllerClass __state, ref AbstractQuestControllerClass ____questController)
        {
            __state = ____questController;
            ____questController = null;
        }

        [PatchPostfix]
        private static void PatchPostfix(AbstractQuestControllerClass __state, ref AbstractQuestControllerClass ____questController)
        {
            ____questController = __state;
        }
    }
}
