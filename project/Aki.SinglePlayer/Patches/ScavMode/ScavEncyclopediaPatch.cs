using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.SinglePlayer.Utils.InRaid;
using EFT;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.ScavMode
{
    /**
     * At the start of a scav raid, copy the PMC encyclopedia to the scav profile, and
     * make sure the scav knows all of the items it has in its inventory
     */
    internal class ScavEncyclopediaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            if (RaidChangesUtil.IsScavRaid)
            {
                var scavProfile = PatchConstants.BackEndSession.ProfileOfPet;
                var pmcProfile = PatchConstants.BackEndSession.Profile;

                // Handle old profiles where the scav doesn't have an encyclopedia
                if (scavProfile.Encyclopedia == null)
                {
                    scavProfile.Encyclopedia = new Dictionary<string, bool>();
                }

                // Sync the PMC encyclopedia to the scav profile
                foreach (var item in pmcProfile.Encyclopedia.Where(item => !scavProfile.Encyclopedia.ContainsKey(item.Key)))
                {
                    scavProfile.Encyclopedia.Add(item.Key, item.Value);
                }

                // Auto examine any items the scav doesn't know that are in their inventory
                scavProfile.LearnAll();
            }
        }
    }
}
