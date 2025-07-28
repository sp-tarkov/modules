using System.Linq;
using System.Reflection;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Copy over scav-only quests and wishlist from PMC profile to scav profile on pre-raid screen
    /// Allows scavs to see and complete quests and see favourited items
    /// </summary>
    public class CopyPmcQuestsAndWishlistToPlayerScavPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(MatchmakerOfflineRaidScreen))
                .SingleCustom(m => m.Name == nameof(MatchmakerOfflineRaidScreen.Show) && m.GetParameters().Length == 1);
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            var pmcProfile = PatchConstants.BackEndSession.Profile;
            var scavProfile = PatchConstants.BackEndSession.ProfileOfPet;

            // Iterate over all quests on pmc that are flagged as being for scavs
            foreach (var quest in pmcProfile.QuestsData.Where(x => x.Template?.PlayerGroup == EFT.EPlayerGroup.Scav))
            {
                // If quest doesn't exist in scav but does in pmc, add it
                var any = false;
                foreach (var questInProfile in scavProfile.QuestsData)
                {
                    if (questInProfile.Id == quest.Id)
                    {
                        any = true;
                        break;
                    }
                }

                if (!any)
                {
                    scavProfile.QuestsData.Add(quest);
                }
            }

            // Copy over all wishlist items from pmc to scav
            foreach (var KvP in pmcProfile.WishlistManager.GetWishlist())
            {
                if (scavProfile.WishlistManager.GetWishlist().ContainsKey(KvP.Key))
                {
                    continue;
                }

                scavProfile.WishlistManager.AddToWishlist(KvP.Key, KvP.Value, true);
            }
        }
    }
}
