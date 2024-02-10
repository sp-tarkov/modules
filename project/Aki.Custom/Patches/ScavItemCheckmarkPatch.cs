using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.UI.DragAndDrop;
using HarmonyLib;

namespace Aki.Custom.Patches
{

    public class ScavItemCheckmarkPatch : ModulePatch
    {
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(QuestItemViewPanel), nameof(QuestItemViewPanel.smethod_0));
		}

		[PatchPrefix]
		public static void PatchPreFix(ref IEnumerable<QuestDataClass> quests)
		{
			var pmcQuests = PatchConstants.BackEndSession.Profile.QuestsData;
			var scavQuests = PatchConstants.BackEndSession.ProfileOfPet.QuestsData;

			quests = pmcQuests.Concat(scavQuests);
		}
	}
}