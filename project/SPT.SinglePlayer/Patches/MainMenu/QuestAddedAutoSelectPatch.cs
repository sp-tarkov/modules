using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// BSG added a new handler that's called when a new quest is added to the quest list, this handler results in the first quest in
    /// a trader's task list being selected whenever a new quest is added. This has the negative effect of breaking replacing dailies
    /// for a trader with a daily from that same trader, due to the selection happening _before_ the daily is removed. Essentially 
    /// resulting in a race condition where the code to remove the old daily tries to remove the trader's first quest instead.
    /// 
    /// We can work around it by conditioning the call to `AutoSelectQuest` in QuestAddedHandler. This does cause a change in behaviour
    /// from live, but fixes the client freezing when getting a new daily from the trader you're replacing the daily on.
    /// 
    /// Note: While this fixes a BSG bug, we can't avoid fixing it, because it causes the UI to freeze when replacing daily quests
    ///       We can retire this patch once BSG fixes the underlying bug.
    ///       
    /// Testing: You can test if this issue still occurs by disabling this patch, creating a Bear SPT Dev profile, and accepting then
    ///          replacing a daily task on Fence. If the UI doesn't freeze up, BSG has fixed the bug
    /// </summary>
    internal class QuestAddedAutoSelectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(QuestsListView), nameof(QuestsListView.QuestAddedHandler));
        }

        [PatchPrefix]
        public static bool PatchPrefix(QuestsListView __instance, QuestListItem ____questListItemSelected, QuestListItem view)
        {
            // The content of this patch should match QuestsListView::QuestAddedHandler, with the call to `AutoSelectQuest` wrapped in a null check
            __instance.UpdateSingleQuestVisibility(view);

            // The only reason I can think to call AutoSelectQuest would be if the current selected quest is null, to select the first quest
            // added to the list, if you somehow get a quest while on the trader task list
            if (____questListItemSelected == null)
            {
                __instance.AutoSelectQuest();
            }

            return false;
        }
    }
}
