using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.UI.Matchmaker;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.ScavMode
{
    /// <summary>
    /// Copy over scav-only quests from PMC profile to scav profile on pre-raid screen
    /// Allows scavs to see and complete quests
    /// </summary>
    public class ScavQuestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MatchmakerOfflineRaidScreen);
            var desiredMethod = desiredType.GetMethod(nameof(MatchmakerOfflineRaidScreen.Show));

            Logger.LogDebug($"{GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var pmcProfile = PatchConstants.BackEndSession.Profile;
            var scavProfile = PatchConstants.BackEndSession.ProfileOfPet;

            // Iterate over all quests on pmc that are flagged as being for scavs
            foreach (var quest in pmcProfile.QuestsData.Where(x => x.Template?.PlayerGroup == EFT.EPlayerGroup.Scav))
            {
                // If quest doesnt exist in scav, add it
                if (!scavProfile.QuestsData.Any(x => x.Id == quest.Id))
                {
                    scavProfile.QuestsData.Add(quest);
                }
            }
        }
    }
}