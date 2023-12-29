using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Progression
{
    /// <summary>
    /// After picking up a quest item, trigger CheckForStatusChange() from the questController to fully update a quest subtasks to show (e.g. `survive and extract item from raid` task)
    /// </summary>
    public class MidRaidQuestChangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Profile).GetMethod("AddToCarriedQuestItems", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld != null)
            {
                var player = gameWorld.MainPlayer;

                var questController = Traverse.Create(player).Field<GClass3201>("_questController").Value;
                if (questController != null)
                {
                    foreach (var quest in questController.Quests.ToList())
                    {
                        quest.CheckForStatusChange(true, true);
                    }
                }
            }
            
        }
    }
}