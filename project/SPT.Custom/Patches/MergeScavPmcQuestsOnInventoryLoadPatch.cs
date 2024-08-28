using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.UI.DragAndDrop;
using HarmonyLib;

namespace SPT.Custom.Patches
{

    public class MergeScavPmcQuestsOnInventoryLoadPatch : ModulePatch
    {	
		/// <summary>
		/// This patch runs both in raid and on main Menu everytime the inventory is loaded
		/// Aim is to let Scavs see what required items your PMC needs for quests like Live using the FiR status
		/// </summary>
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(QuestItemViewPanel), nameof(QuestItemViewPanel.smethod_0));
		}

		[PatchPrefix]
		public static void PatchPreFix(ref IEnumerable<QuestDataClass> quests)
		{
			var gameWorld = Singleton<GameWorld>.Instance;
			if (gameWorld != null)
			{
				if (gameWorld.MainPlayer.Location != "hideout" && gameWorld.MainPlayer.Fraction == ETagStatus.Scav)
				{
					var pmcQuests = PatchConstants.BackEndSession.Profile.QuestsData;
					var scavQuests = PatchConstants.BackEndSession.ProfileOfPet.QuestsData;
					quests = pmcQuests.Concat(scavQuests);
				}
			}	
		}
	}
}